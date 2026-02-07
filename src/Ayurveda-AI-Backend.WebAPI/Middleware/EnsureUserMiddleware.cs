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
    // In-memory cache of known user IDs to avoid hitting DB on every request
    private static readonly ConcurrentDictionary<Guid, bool> _knownUsers = new();

    public EnsureUserMiddleware(RequestDelegate next)
    {
        _next = next;
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
                    await EnsureUserExistsAsync(context, userId);
                }
            }
        }

        await _next(context);
    }

    private static async Task EnsureUserExistsAsync(HttpContext context, Guid userId)
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

        var user = new User
        {
            Id = userId,
            Email = email,
            AuthProvider = AuthProvider.Email,
            AuthProviderUserId = userId.ToString(),
            IsEmailVerified = true, // already verified through Supabase OTP
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastSignInAt = DateTime.UtcNow,
        };

        try
        {
            await userRepo.AddAsync(user);
            _knownUsers.TryAdd(userId, true);
        }
        catch (Exception)
        {
            // Race condition: another request created the user first — that's fine
            _knownUsers.TryAdd(userId, true);
        }
    }
}
