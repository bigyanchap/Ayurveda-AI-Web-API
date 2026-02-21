using System.Net.Http.Json;
using System.Text.Json;
using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Ayurveda_AI_Backend.Service.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ayurveda_AI_Backend.Service.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly IRepository<UserProfile> _profileRepository;
    private readonly IRepository<PrakritiResult> _prakritiRepository;
    private readonly IRepository<HealthIndicator> _indicatorRepository;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(
        HttpClient httpClient,
        IOptions<GeminiOptions> options,
        IRepository<UserProfile> profileRepository,
        IRepository<PrakritiResult> prakritiRepository,
        IRepository<HealthIndicator> indicatorRepository,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _profileRepository = profileRepository;
        _prakritiRepository = prakritiRepository;
        _indicatorRepository = indicatorRepository;
        _logger = logger;
    }

    public async Task<ChatResponseDto> GetChatResponseAsync(ChatRequestDto request)
    {
        var (profile, prakriti, indicators) = await LoadUserHealthData(request.UserId);

        var timeOfDay = string.IsNullOrWhiteSpace(request.TimeOfDay)
            ? DeriveTimeOfDay()
            : request.TimeOfDay;
        var weather = request.Weather ?? "Unknown";
        var location = request.Location ?? profile?.Country ?? "Unknown";

        var systemPrompt = GeminiPromptBuilder.BuildChatSystemPrompt(
            profile, prakriti, indicators, timeOfDay, weather, location);

        _logger.LogInformation("Generating chat response for user {UserId} (history: {HistoryCount} messages)",
            request.UserId, request.History?.Count ?? 0);

        var responseText = await CallGeminiMultiTurnAsync(systemPrompt, request.History, request.Message);
        return new ChatResponseDto(responseText, DateTime.UtcNow);
    }

    public async Task<GenerateArticlesResponseDto> GenerateArticlesAsync(GenerateArticlesRequestDto request)
    {
        var (profile, prakriti, indicators) = await LoadUserHealthData(request.UserId);

        var timeOfDay = string.IsNullOrWhiteSpace(request.TimeOfDay)
            ? DeriveTimeOfDay()
            : request.TimeOfDay;
        var weather = string.IsNullOrWhiteSpace(request.Weather)
            ? "Unknown"
            : request.Weather;
        var location = string.IsNullOrWhiteSpace(request.Location)
            ? profile?.Country ?? "Unknown"
            : request.Location;

        var prompt = GeminiPromptBuilder.BuildArticlePrompt(
            profile, prakriti, indicators, timeOfDay, weather, location);

        _logger.LogInformation("Generating articles for user {UserId}", request.UserId);
        var responseText = await CallGeminiAsync(prompt);
        return new GenerateArticlesResponseDto(responseText, DateTime.UtcNow);
    }

    // ── Private helpers ───────────────────────────────────────────────

    private async Task<(UserProfile?, PrakritiResult?, IReadOnlyList<HealthIndicator>)>
        LoadUserHealthData(Guid userId)
    {
        var profile = (await _profileRepository.FindAsync(p => p.UserId == userId)).FirstOrDefault();

        var prakriti = (await _prakritiRepository.FindAsync(p => p.UserId == userId && p.IsActive))
            .OrderByDescending(p => p.CalculatedAt)
            .FirstOrDefault();

        var indicators = (await _indicatorRepository.FindAsync(i => i.UserId == userId && i.IsActive))
            .ToList();

        return (profile, prakriti, indicators);
    }

    private static string DeriveTimeOfDay()
    {
        return DateTime.Now.Hour switch
        {
            >= 2 and < 6 => "Early Morning (Brahma-Muhurta, 2am-6am)",
            >= 6 and < 10 => "Morning (6am-10am)",
            >= 10 and < 14 => "Midday (10am-2pm)",
            >= 14 and < 18 => "Afternoon (2pm-6pm)",
            >= 18 and < 20 => "Evening (6pm-8pm)",
            _ => "Night (8pm-2pm)"
        };
    }

    /// <summary>
    /// Call Gemini with system instruction + multi-turn conversation history.
    /// </summary>
    private async Task<string> CallGeminiMultiTurnAsync(
        string systemPrompt,
        IReadOnlyList<ChatHistoryItemDto>? history,
        string currentMessage)
    {
        var url = $"{_options.Endpoint}/{_options.Model}:generateContent?key={_options.ApiKey}";

        // Build the contents array: history turns + current user message
        var contents = new List<object>();

        if (history != null)
        {
            foreach (var item in history)
            {
                var role = item.Role == "ai" ? "model" : "user";
                contents.Add(new { role, parts = new[] { new { text = item.Text } } });
            }
        }

        // Current user message
        contents.Add(new { role = "user", parts = new[] { new { text = currentMessage } } });

        var payload = new
        {
            system_instruction = new { parts = new[] { new { text = systemPrompt } } },
            contents
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync(url, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach Gemini API");
            return "I'm having trouble connecting to my AI service right now. Please try again in a moment.";
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Gemini API returned {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
            response.Dispose();
            return "I could not generate a response right now. Please try again.";
        }

        try
        {
            using (response)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(responseStream);
                var text = document
                    .RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return string.IsNullOrWhiteSpace(text)
                    ? "I could not generate a response right now. Please try again."
                    : text;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response");
            return "I could not generate a response right now. Please try again.";
        }
    }

    private async Task<string> CallGeminiAsync(string prompt)
    {
        var url = $"{_options.Endpoint}/{_options.Model}:generateContent?key={_options.ApiKey}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync(url, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach Gemini API");
            return "I'm having trouble connecting to my AI service right now. Please try again in a moment.";
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Gemini API returned {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
            response.Dispose();
            return "I could not generate a response right now. Please try again.";
        }

        try
        {
            using (response)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(responseStream);
                var text = document
                    .RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("Gemini returned empty response");
                }

                return string.IsNullOrWhiteSpace(text)
                    ? "I could not generate a response right now. Please try again."
                    : text;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response");
            return "I could not generate a response right now. Please try again.";
        }
    }
}
