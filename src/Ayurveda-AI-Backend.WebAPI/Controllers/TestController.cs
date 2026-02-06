using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Domain.Enums;
using Ayurveda_AI_Backend.Infrastructure.Supabase;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Supabase.Gotrue.Exceptions;

namespace Ayurveda_AI_Backend.WebAPI.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<AccessPolicy> _policyRepository;
    private readonly IRepository<Coupon> _couponRepository;
    private readonly IRepository<HealthIndicator> _indicatorRepository;
    private readonly IRepository<PoopType> _poopTypeRepository;
    private readonly IRepository<EnergyLevel> _energyLevelRepository;
    private readonly IRepository<QuizQuestion> _quizRepository;
    private readonly IRepository<GeminiQuestion> _geminiQuestionRepository;
    private readonly IRepository<ChronicCondition> _chronicConditionRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TestController> _logger;
    private readonly ISupabaseClientProvider _supabaseClientProvider;
    private readonly SupabaseOptions _supabaseOptions;

    public TestController(
        IRepository<User> userRepository,
        IRepository<AccessPolicy> policyRepository,
        IRepository<Coupon> couponRepository,
        IRepository<HealthIndicator> indicatorRepository,
        IRepository<PoopType> poopTypeRepository,
        IRepository<EnergyLevel> energyLevelRepository,
        IRepository<QuizQuestion> quizRepository,
        IRepository<GeminiQuestion> geminiQuestionRepository,
        IRepository<ChronicCondition> chronicConditionRepository,
        IWebHostEnvironment environment,
        ILogger<TestController> logger,
        ISupabaseClientProvider supabaseClientProvider,
        IOptions<SupabaseOptions> supabaseOptions)
    {
        _userRepository = userRepository;
        _policyRepository = policyRepository;
        _couponRepository = couponRepository;
        _indicatorRepository = indicatorRepository;
        _poopTypeRepository = poopTypeRepository;
        _energyLevelRepository = energyLevelRepository;
        _quizRepository = quizRepository;
        _geminiQuestionRepository = geminiQuestionRepository;
        _chronicConditionRepository = chronicConditionRepository;
        _environment = environment;
        _logger = logger;
        _supabaseClientProvider = supabaseClientProvider;
        _supabaseOptions = supabaseOptions.Value;
    }

    [HttpPost("users")]
    [AllowAnonymous]
    public async Task<ActionResult<TestUserResponse>> CreateTestUser([FromBody] CreateTestUserRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Password is required.");
        }

        var normalizedEmail = request.Email.Trim();
        var password = request.Password!;

        if (string.IsNullOrWhiteSpace(_supabaseOptions.Url) || string.IsNullOrWhiteSpace(_supabaseOptions.ApiKey))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Supabase configuration is missing.");
        }

        await _supabaseClientProvider.Client.InitializeAsync();

        string? supabaseUserId = null;
        try
        {
            var admin = _supabaseClientProvider.Client.AdminAuth(_supabaseOptions.ApiKey);
            dynamic? created = await admin.CreateUser(
                normalizedEmail,
                password,
                new Supabase.Gotrue.AdminUserAttributes
                {
                    EmailConfirm = true
                });

            supabaseUserId = ExtractUserId(created);
        }
        catch (GotrueException ex) when (ex.Message.Contains("email_exists", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                dynamic? session = await _supabaseClientProvider.Client.Auth.SignIn(normalizedEmail, password);
                supabaseUserId = ExtractUserId(session);
            }
            catch (Exception signInEx)
            {
                _logger.LogError(signInEx, "Supabase sign-in failed for {Email}", normalizedEmail);
                return StatusCode(StatusCodes.Status502BadGateway, "Supabase sign-in failed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Supabase signup failed for {Email}", normalizedEmail);
            return StatusCode(StatusCodes.Status502BadGateway, "Supabase signup failed.");
        }

        if (string.IsNullOrWhiteSpace(supabaseUserId) && !string.IsNullOrWhiteSpace(request.AuthProviderUserId))
        {
            supabaseUserId = request.AuthProviderUserId.Trim();
        }

        User? existing;
        try
        {
            existing = (await _userRepository.FindAsync(u => u.Email == normalizedEmail)).FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database query failed for {Email}", normalizedEmail);
            return StatusCode(StatusCodes.Status502BadGateway, "Database query failed.");
        }

        if (existing != null)
        {
            return Ok(new TestUserResponse(existing.Id, existing.Email, false, supabaseUserId));
        }

        if (string.IsNullOrWhiteSpace(supabaseUserId))
        {
            return Conflict("Supabase user already exists. Provide authProviderUserId to link.");
        }

        var userId = Guid.TryParse(supabaseUserId, out var parsedId) ? parsedId : Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = normalizedEmail,
            AuthProvider = AuthProvider.Email,
            AuthProviderUserId = supabaseUserId ?? request.AuthProviderUserId?.Trim() ?? normalizedEmail,
            IsEmailVerified = true,
            IsActive = true
        };

        try
        {
            await _userRepository.AddAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert user record for {Email}", normalizedEmail);
            return StatusCode(StatusCodes.Status502BadGateway, "Database insert failed.");
        }

        _logger.LogInformation("Created test user {UserId} for email {Email}", user.Id, user.Email);

        return Created($"/api/users/{user.Id}/profile", new TestUserResponse(user.Id, user.Email, true, supabaseUserId));
    }

    [HttpPost("seed")]
    [AllowAnonymous]
    public async Task<ActionResult<TestSeedResponse>> SeedTestData()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var policy = (await _policyRepository.FindAsync(p => p.PolicyType == PolicyType.Free)).FirstOrDefault();
        if (policy == null)
        {
            policy = new AccessPolicy
            {
                Id = Guid.NewGuid(),
                PolicyType = PolicyType.Free,
                MaxChatsPerDay = 5,
                MaxArticlesPerDay = 5,
                IsActive = true
            };
            await _policyRepository.AddAsync(policy);
        }

        var coupon = (await _couponRepository.FindAsync(c => c.Code == "TEST10")).FirstOrDefault();
        if (coupon == null)
        {
            coupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "TEST10",
                PlanType = PlanType.Trial,
                MaxRedemptions = 10,
                RedeemedCount = 0,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                IsActive = true
            };
            await _couponRepository.AddAsync(coupon);
        }

        var indicator = (await _indicatorRepository.FindAsync(i => i.Name == "Energy")).FirstOrDefault();
        if (indicator == null)
        {
            indicator = new HealthIndicator
            {
                Id = Guid.NewGuid(),
                Name = "Energy",
                Description = "Daily energy level",
                Category = "General",
                IsActive = true
            };
            await _indicatorRepository.AddAsync(indicator);
        }

        var poopType = (await _poopTypeRepository.FindAsync(p => p.Name == "Normal")).FirstOrDefault();
        if (poopType == null)
        {
            poopType = new PoopType
            {
                Id = Guid.NewGuid(),
                Name = "Normal",
                Description = "Balanced and regular"
            };
            await _poopTypeRepository.AddAsync(poopType);
        }

        var energyLevel = (await _energyLevelRepository.FindAsync(e => e.Name == "High")).FirstOrDefault();
        if (energyLevel == null)
        {
            energyLevel = new EnergyLevel
            {
                Id = Guid.NewGuid(),
                Name = "High",
                Description = "High energy"
            };
            await _energyLevelRepository.AddAsync(energyLevel);
        }

        var quizQuestion = (await _quizRepository.FindAsync(q => q.QuestionText == "How do you feel today?"))
            .FirstOrDefault();
        if (quizQuestion == null)
        {
            quizQuestion = new QuizQuestion
            {
                Id = Guid.NewGuid(),
                Category = "General",
                QuestionText = "How do you feel today?",
                IsActive = true,
                Options = new List<QuizOption>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        OptionText = "Great",
                        OptionValue = "great"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        OptionText = "Okay",
                        OptionValue = "okay"
                    }
                }
            };
            await _quizRepository.AddAsync(quizQuestion);
        }

        var geminiQuestion = (await _geminiQuestionRepository.FindAsync(q => q.QuestionText == "What is your focus?"))
            .FirstOrDefault();
        if (geminiQuestion == null)
        {
            geminiQuestion = new GeminiQuestion
            {
                Id = Guid.NewGuid(),
                QuestionText = "What is your focus?",
                Category = "General",
                IsActive = true
            };
            await _geminiQuestionRepository.AddAsync(geminiQuestion);
        }

        return Ok(new TestSeedResponse(
            policy.Id,
            coupon.Id,
            indicator.Id,
            poopType.Id,
            energyLevel.Id,
            quizQuestion.Id,
            geminiQuestion.Id));
    }

    [HttpPost("chronic-conditions")]
    [AllowAnonymous]
    public async Task<ActionResult<ChronicConditionDto>> CreateChronicCondition([FromBody] CreateChronicConditionRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var condition = new ChronicCondition
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ConditionType = request.ConditionType.Trim(),
            Severity = request.Severity.Trim(),
            DiagnosedAt = request.DiagnosedAt,
            IsActive = true
        };

        await _chronicConditionRepository.AddAsync(condition);
        return Created($"/api/test/chronic-conditions/{condition.Id}", MapCondition(condition));
    }

    [HttpGet("chronic-conditions")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ChronicConditionDto>>> ListChronicConditions([FromQuery] Guid userId)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var conditions = await _chronicConditionRepository.FindAsync(c => c.UserId == userId);
        return Ok(conditions.Select(MapCondition).ToList());
    }

    [HttpGet("chronic-conditions/{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ChronicConditionDto>> GetChronicCondition(Guid id)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var condition = await _chronicConditionRepository.GetByIdAsync(id);
        if (condition == null)
        {
            return NotFound();
        }

        return Ok(MapCondition(condition));
    }

    [HttpPut("chronic-conditions/{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ChronicConditionDto>> UpdateChronicCondition(Guid id, [FromBody] UpdateChronicConditionRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var condition = await _chronicConditionRepository.GetByIdAsync(id);
        if (condition == null)
        {
            return NotFound();
        }

        condition.ConditionType = request.ConditionType.Trim();
        condition.Severity = request.Severity.Trim();
        condition.DiagnosedAt = request.DiagnosedAt;
        condition.IsActive = request.IsActive;

        await _chronicConditionRepository.UpdateAsync(condition);
        return Ok(MapCondition(condition));
    }

    [HttpDelete("chronic-conditions/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteChronicCondition(Guid id)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var condition = await _chronicConditionRepository.GetByIdAsync(id);
        if (condition == null)
        {
            return NotFound();
        }

        await _chronicConditionRepository.DeleteAsync(condition);
        return NoContent();
    }

    private static ChronicConditionDto MapCondition(ChronicCondition condition)
    {
        return new ChronicConditionDto(
            condition.Id,
            condition.UserId,
            condition.ConditionType,
            condition.Severity,
            condition.DiagnosedAt,
            condition.IsActive);
    }

    private static string? ExtractUserId(dynamic? result)
    {
        if (result == null)
        {
            return null;
        }

        try
        {
            return (string?)result.Id;
        }
        catch
        {
        }

        try
        {
            return (string?)result.User?.Id;
        }
        catch
        {
        }

        try
        {
            return (string?)result.UserId;
        }
        catch
        {
        }

        return null;
    }
}

public sealed record CreateTestUserRequest(string Email, string? Password, string? AuthProviderUserId);

public sealed record TestUserResponse(Guid Id, string Email, bool Created, string? SupabaseUserId);

public sealed record TestSeedResponse(
    Guid AccessPolicyId,
    Guid CouponId,
    Guid HealthIndicatorId,
    Guid PoopTypeId,
    Guid EnergyLevelId,
    Guid QuizQuestionId,
    Guid GeminiQuestionId);

public sealed record CreateChronicConditionRequest(
    Guid UserId,
    string ConditionType,
    string Severity,
    DateTime? DiagnosedAt);

public sealed record UpdateChronicConditionRequest(
    string ConditionType,
    string Severity,
    DateTime? DiagnosedAt,
    bool IsActive);

public sealed record ChronicConditionDto(
    Guid Id,
    Guid UserId,
    string ConditionType,
    string Severity,
    DateTime? DiagnosedAt,
    bool IsActive);
