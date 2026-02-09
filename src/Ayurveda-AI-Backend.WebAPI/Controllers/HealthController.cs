using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Domain.Enums;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Ayurveda_AI_Backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayurveda_AI_Backend.WebAPI.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;
    private readonly IRepository<QuizQuestion> _quizRepository;
    private readonly IRepository<GeminiQuestion> _geminiQuestionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IHealthService healthService,
        IRepository<QuizQuestion> quizRepository,
        IRepository<GeminiQuestion> geminiQuestionRepository,
        IRepository<User> userRepository,
        ILogger<HealthController> logger)
    {
        _healthService = healthService;
        _quizRepository = quizRepository;
        _geminiQuestionRepository = geminiQuestionRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet("get-health-result/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<PrakritiResultDto>> GetPrakritiResult(Guid userId)
    {
        var result1 = await _healthService.GetPrakritiResultAsync(userId);
        var result2 = await _healthService.GetIndicatorsAsync(userId);
        if (result1 == null && result2 == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            PrakritiResult = result1,
            Indicators = result2
        });
    }

    [HttpPost("post-prakriti")]
    [Authorize]
    public async Task<ActionResult<PrakritiResultDto>> SavePrakriti([FromBody] PrakritiResultDto dto)
    {
        // Extract the authenticated user's ID from the JWT "sub" claim
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user identity." });

        try
        {
            // Ensure the local User row exists (FK requirement)
            await EnsureLocalUserExists(userId);

            var safeDto = dto with { UserId = userId };
            var result = await _healthService.SavePrakritiResultAsync(safeDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save prakriti result for user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to save prakriti result.", detail = ex.Message });
        }
    }

    [HttpPost("post-indicators")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<HealthIndicatorDto>>> SaveIndicators(
        [FromBody] IReadOnlyList<HealthIndicatorDto> dto)
    {
        // Extract the authenticated user's ID from the JWT "sub" claim
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user identity." });

        try
        {
            // Ensure the local User row exists (FK requirement)
            await EnsureLocalUserExists(userId);

            // Override userId on every indicator with the authenticated user's ID
            var safeDtos = dto.Select(d => d with { UserId = userId }).ToList();
            var result = await _healthService.SaveIndicatorsAsync(safeDtos);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save health indicators for user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to save health indicators.", detail = ex.Message });
        }
    }

    [HttpGet("get-indicators/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<HealthIndicatorDto>>> GetIndicators(Guid userId)
    {
        var indicators = await _healthService.GetIndicatorsAsync(userId);
        return Ok(indicators);
    }

    [HttpGet("quiz-questions")]
    public async Task<ActionResult<IReadOnlyList<QuizQuestion>>> GetQuizQuestions()
    {
        var questions = await _quizRepository.GetAllAsync();
        return Ok(questions);
    }

    [HttpGet("gemini-questions")]
    public async Task<ActionResult<IReadOnlyList<GeminiQuestion>>> GetGeminiQuestions()
    {
        var questions = await _geminiQuestionRepository.GetAllAsync();
        return Ok(questions);
    }

    /// <summary>
    /// Guarantee the local User row exists before inserting child records.
    /// Handles the case where the same email was registered multiple times
    /// in Supabase (different UUIDs) — updates the existing row's Id.
    /// </summary>
    private async Task EnsureLocalUserExists(Guid userId)
    {
        // 1. Already exists with this exact Id? Nothing to do.
        var existing = await _userRepository.GetByIdAsync(userId);
        if (existing != null) return;

        var email = User.FindFirst("email")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? $"user-{userId}@supabase.local";

        // 2. A row with the same email but a DIFFERENT Id?
        //    This happens when the user re-registered in Supabase (new UUID, same email).
        var byEmail = (await _userRepository.FindAsync(u => u.Email == email)).FirstOrDefault();
        if (byEmail != null)
        {
            _logger.LogWarning(
                "User row exists for email {Email} with old Id {OldId}, but JWT has new Id {NewId}. " +
                "Deleting old row and creating fresh one.",
                email, byEmail.Id, userId);

            // Delete the stale row (it has no child records since saves always failed)
            await _userRepository.DeleteAsync(byEmail);

            // Create with the correct Id
            var replacement = new User
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
            await _userRepository.AddAsync(replacement);
            _logger.LogInformation("Replaced User row: new Id {UserId} for {Email}.", userId, email);
            return;
        }

        // 3. Completely new user — create from scratch.
        _logger.LogWarning("User {UserId} not found in DB — creating before saving health data.", userId);

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

        await _userRepository.AddAsync(user);
        _logger.LogInformation("Created User {UserId} ({Email}) from HealthController.", userId, email);
    }
}
