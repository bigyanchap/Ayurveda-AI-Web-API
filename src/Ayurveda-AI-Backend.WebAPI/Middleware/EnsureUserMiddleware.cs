using System.Collections.Concurrent;
using System.Security.Claims;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Domain.Enums;
using Ayurveda_AI_Backend.Repository.Interfaces;

namespace Ayurveda_AI_Backend.WebAPI.Middleware;

/// <summary>
/// Ensures that an authenticated Supabase user has a corresponding
/// local User record in the database. Runs after authentication
/// and auto-creates the User row on first request.
/// </summary>
public class EnsureUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EnsureUserMiddleware> _logger;
    // In-memory cache of known user IDs to avoid hitting DB on every request
    private static readonly ConcurrentDictionary<Guid, bool> _knownUsers = new();

    public EnsureUserMiddleware(RequestDelegate next, ILogger<EnsureUserMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Supabase JWT has "sub" claim with user UUID
            var subClaim = context.User.FindFirst("sub")
                ?? context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (subClaim != null && Guid.TryParse(subClaim.Value, out var userId))
            {
                // Only hit DB if we haven't seen this user in this process lifetime
                if (!_knownUsers.ContainsKey(userId))
                {
                    _logger.LogInformation("EnsureUserMiddleware: User {UserId} not in cache, checking DB...", userId);
                    await EnsureUserExistsAsync(context, userId);
                }
            }
            else
            {
                _logger.LogWarning("EnsureUserMiddleware: Authenticated but no 'sub' claim found. Claims: {Claims}",
                    string.Join(", ", context.User.Claims.Select(c => $"{c.Type}={c.Value}")));
            }
        }

        await _next(context);
    }

    private async Task EnsureUserExistsAsync(HttpContext context, Guid userId)
    {
        using var scope = context.RequestServices.CreateScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<User>>();

        var existing = await userRepo.GetByIdAsync(userId);
        if (existing != null)
        {
            _knownUsers.TryAdd(userId, true);
            return;
        }

        // Extract email from JWT claims
        var email = context.User.FindFirst("email")?.Value
            ?? context.User.FindFirst(ClaimTypes.Email)?.Value
            ?? $"user-{userId}@supabase.local";

        _logger.LogInformation("EnsureUserMiddleware: Creating User {UserId} ({Email})...", userId, email);

        // Check if a row with the same email but different Id already exists
        // (happens when user re-registered in Supabase, getting a new UUID)
        var byEmail = (await userRepo.FindAsync(u => u.Email == email)).FirstOrDefault();
        if (byEmail != null)
        {
            _logger.LogWarning(
                "EnsureUserMiddleware: Stale User row for {Email} (old Id {OldId}). Replacing with {NewId}.",
                email, byEmail.Id, userId);
            await userRepo.DeleteAsync(byEmail);
        }

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
            await userRepo.AddAsync(user);
            _knownUsers.TryAdd(userId, true);
            _logger.LogInformation("EnsureUserMiddleware: Created User {UserId}.", userId);
        }
        catch (Exception ex)
        {
            var recheck = await userRepo.GetByIdAsync(userId);
            if (recheck != null)
            {
                _knownUsers.TryAdd(userId, true);
            }
            else
            {
                _logger.LogError(ex, "EnsureUserMiddleware: FAILED to create User {UserId}!", userId);
            }
        }
    }
}
