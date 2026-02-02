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
    private readonly IRepository<VikritiSnapshot> _vikritiRepository;
    private readonly IRepository<HealthSignal> _signalRepository;
    private readonly IRepository<ChronicCondition> _conditionRepository;
    private readonly IRepository<UserLifestyleProfile> _lifestyleRepository;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(
        HttpClient httpClient,
        IOptions<GeminiOptions> options,
        IRepository<UserProfile> profileRepository,
        IRepository<PrakritiResult> prakritiRepository,
        IRepository<VikritiSnapshot> vikritiRepository,
        IRepository<HealthSignal> signalRepository,
        IRepository<ChronicCondition> conditionRepository,
        IRepository<UserLifestyleProfile> lifestyleRepository,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _profileRepository = profileRepository;
        _prakritiRepository = prakritiRepository;
        _vikritiRepository = vikritiRepository;
        _signalRepository = signalRepository;
        _conditionRepository = conditionRepository;
        _lifestyleRepository = lifestyleRepository;
        _logger = logger;
    }

    public async Task<ChatResponseDto> GetChatResponseAsync(ChatRequestDto request)
    {
        var profile = (await _profileRepository.FindAsync(p => p.UserId == request.UserId)).FirstOrDefault();
        var prakriti = (await _prakritiRepository.FindAsync(p => p.UserId == request.UserId && p.IsActive))
            .OrderByDescending(p => p.CalculatedAt)
            .FirstOrDefault();
        var vikriti = (await _vikritiRepository.FindAsync(v => v.UserId == request.UserId))
            .OrderByDescending(v => v.CalculatedAt)
            .FirstOrDefault();
        var recentSignals = (await _signalRepository.FindAsync(s => s.UserId == request.UserId))
            .OrderByDescending(s => s.ReportedAt)
            .Take(5)
            .ToList();

        var prompt = GeminiPromptBuilder.BuildChatPrompt(profile, prakriti, vikriti, recentSignals, request.Message);
        _logger.LogInformation("Generating chat response for user {UserId}", request.UserId);
        var responseText = await CallGeminiAsync(prompt);

        return new ChatResponseDto(responseText, DateTime.UtcNow);
    }

    public async Task<GenerateArticlesResponseDto> GenerateArticlesAsync(GenerateArticlesRequestDto request)
    {
        var profile = (await _profileRepository.FindAsync(p => p.UserId == request.UserId)).FirstOrDefault();
        var prakriti = (await _prakritiRepository.FindAsync(p => p.UserId == request.UserId && p.IsActive))
            .OrderByDescending(p => p.CalculatedAt)
            .FirstOrDefault();
        var vikriti = (await _vikritiRepository.FindAsync(v => v.UserId == request.UserId))
            .OrderByDescending(v => v.CalculatedAt)
            .FirstOrDefault();
        var signals = await _signalRepository.FindAsync(s => s.UserId == request.UserId);
        var conditions = await _conditionRepository.FindAsync(c => c.UserId == request.UserId && c.IsActive);
        var lifestyle = (await _lifestyleRepository.FindAsync(l => l.UserId == request.UserId))
            .OrderByDescending(l => l.Id)
            .FirstOrDefault();

        var prompt = GeminiPromptBuilder.BuildArticlePrompt(profile, prakriti, vikriti, signals, conditions, lifestyle, request);
        _logger.LogInformation("Generating articles for user {UserId}", request.UserId);
        var responseText = await CallGeminiAsync(prompt);

        return new GenerateArticlesResponseDto(responseText, DateTime.UtcNow);
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

        using var response = await _httpClient.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

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
