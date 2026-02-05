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
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TestController> _logger;
    private readonly ISupabaseClientProvider _supabaseClientProvider;
    private readonly SupabaseOptions _supabaseOptions;

    public TestController(
        IRepository<User> userRepository,
        IWebHostEnvironment environment,
        ILogger<TestController> logger,
        ISupabaseClientProvider supabaseClientProvider,
        IOptions<SupabaseOptions> supabaseOptions)
    {
        _userRepository = userRepository;
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
