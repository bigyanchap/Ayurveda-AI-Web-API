using System.Text;
using System.Text.Json;
using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Domain.Enums;
using Ayurveda_AI_Backend.Infrastructure.Supabase;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Ayurveda_AI_Backend.WebAPI.Controllers;

/// <summary>
/// Proxies authentication requests to Supabase GoTrue API.
/// Keeps the service key server-side only.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _supabaseOptions;
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IHttpClientFactory httpClientFactory,
        IOptions<SupabaseOptions> supabaseOptions,
        IRepository<User> userRepository,
        ILogger<AuthController> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _supabaseOptions = supabaseOptions.Value;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Sign up a new user. Supabase sends an OTP verification code to the email.
    /// </summary>
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new AuthMessageDto("Email and password are required."));

        if (request.Password.Length < 6)
            return BadRequest(new AuthMessageDto("Password must be at least 6 characters."));

        var body = new { email = request.Email, password = request.Password };
        var response = await PostToSupabase("/auth/v1/signup", body);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var error = TryParseError(responseBody);
            _logger.LogWarning("Supabase signup failed for {Email}: {Error}", request.Email, error);
            return StatusCode((int)response.StatusCode, new AuthMessageDto(error));
        }

        // Parse the response
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        // Check if identities is empty (email already exists)
        if (root.TryGetProperty("identities", out var identities)
            && identities.ValueKind == JsonValueKind.Array
            && identities.GetArrayLength() == 0)
        {
            return Conflict(new AuthMessageDto("An account with this email already exists."));
        }

        // Check if a session was returned (autoconfirm enabled)
        if (root.TryGetProperty("access_token", out _))
        {
            var authResponse = ParseAuthResponse(root);
            await EnsureLocalUser(authResponse.UserId, request.Email);
            return Ok(authResponse);
        }

        // Email confirmation required — OTP was sent
        return Ok(new AuthMessageDto("Verification code sent to your email. Please check your inbox."));
    }

    /// <summary>
    /// Verify the OTP code sent to the user's email.
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.OtpCode))
            return BadRequest(new AuthMessageDto("Email and verification code are required."));

        var body = new
        {
            type = "signup",
            email = request.Email,
            token = request.OtpCode,
        };
        var response = await PostToSupabase("/auth/v1/verify", body);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var error = TryParseError(responseBody);
            _logger.LogWarning("OTP verification failed for {Email}: {Error}", request.Email, error);
            return StatusCode((int)response.StatusCode, new AuthMessageDto(error));
        }

        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        if (!root.TryGetProperty("access_token", out _))
            return BadRequest(new AuthMessageDto("Verification succeeded but no session was created."));

        var authResponse = ParseAuthResponse(root);
        await EnsureLocalUser(authResponse.UserId, request.Email);
        return Ok(authResponse);
    }

    /// <summary>
    /// Resend the OTP verification code.
    /// </summary>
    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new AuthMessageDto("Email is required."));

        var body = new { type = "signup", email = request.Email };
        var response = await PostToSupabase("/auth/v1/resend", body);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var error = TryParseError(responseBody);
            _logger.LogWarning("Resend OTP failed for {Email}: {Error}", request.Email, error);
            return StatusCode((int)response.StatusCode, new AuthMessageDto(error));
        }

        return Ok(new AuthMessageDto("Verification code resent. Check your inbox."));
    }

    /// <summary>
    /// Sign in with email and password. Returns JWT tokens.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new AuthMessageDto("Email and password are required."));

        var body = new { email = request.Email, password = request.Password };
        var response = await PostToSupabase("/auth/v1/token?grant_type=password", body);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var error = TryParseError(responseBody);

            if (error.Contains("Email not confirmed", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new AuthMessageDto("Please verify your email before signing in. Check your inbox for the verification code."));
            }

            _logger.LogWarning("Login failed for {Email}: {Error}", request.Email, error);
            return StatusCode((int)response.StatusCode, new AuthMessageDto(error));
        }

        using var doc = JsonDocument.Parse(responseBody);
        var authResponse = ParseAuthResponse(doc.RootElement);

        // Update last sign-in
        if (Guid.TryParse(authResponse.UserId, out var userId))
        {
            await EnsureLocalUser(authResponse.UserId, request.Email);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.LastSignInAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
        }

        return Ok(authResponse);
    }

    /// <summary>
    /// Refresh an expired access token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new AuthMessageDto("Refresh token is required."));

        var body = new { refresh_token = request.RefreshToken };
        var response = await PostToSupabase("/auth/v1/token?grant_type=refresh_token", body);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var error = TryParseError(responseBody);
            _logger.LogWarning("Token refresh failed: {Error}", error);
            return StatusCode((int)response.StatusCode, new AuthMessageDto(error));
        }

        using var doc = JsonDocument.Parse(responseBody);
        var authResponse = ParseAuthResponse(doc.RootElement);
        return Ok(authResponse);
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private async Task<HttpResponseMessage> PostToSupabase(string path, object body)
    {
        var url = $"{_supabaseOptions.Url}{path}";
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        request.Headers.Add("apikey", _supabaseOptions.ApiKey);

        return await _httpClient.SendAsync(request);
    }

    private static AuthResponseDto ParseAuthResponse(JsonElement root)
    {
        var accessToken = root.GetProperty("access_token").GetString() ?? "";
        var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() ?? "" : "";
        var expiresIn = root.TryGetProperty("expires_in", out var ei) ? ei.GetInt64() : 3600;

        var userId = "";
        var email = "";
        if (root.TryGetProperty("user", out var user))
        {
            userId = user.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "";
            email = user.TryGetProperty("email", out var em) ? em.GetString() ?? "" : "";
        }

        return new AuthResponseDto(accessToken, refreshToken, userId, email, expiresIn);
    }

    private static string TryParseError(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("msg", out var msg))
                return msg.GetString() ?? "Unknown error";
            if (doc.RootElement.TryGetProperty("error_description", out var desc))
                return desc.GetString() ?? "Unknown error";
            if (doc.RootElement.TryGetProperty("message", out var message))
                return message.GetString() ?? "Unknown error";
        }
        catch { }
        return "An unexpected error occurred.";
    }

    private async Task EnsureLocalUser(string userIdStr, string email)
    {
        if (!Guid.TryParse(userIdStr, out var userId)) return;

        var existing = await _userRepository.GetByIdAsync(userId);
        if (existing != null) return;

        var user = new User
        {
            Id = userId,
            Email = email,
            AuthProvider = AuthProvider.Email,
            AuthProviderUserId = userId.ToString(),
            IsEmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastSignInAt = DateTime.UtcNow,
        };

        try
        {
            await _userRepository.AddAsync(user);
            _logger.LogInformation("Created local User record for {UserId} ({Email})", userId, email);
        }
        catch (Exception ex)
        {
            // Race condition or duplicate — safe to ignore
            _logger.LogDebug(ex, "Could not create user {UserId} — may already exist", userId);
        }
    }
}
