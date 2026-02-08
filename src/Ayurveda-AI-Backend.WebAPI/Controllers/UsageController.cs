using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayurveda_AI_Backend.WebAPI.Controllers;

[ApiController]
[Route("api/usage")]
[Authorize]
public class UsageController : ControllerBase
{
    private readonly IRepository<UserUsage> _usageRepo;
    private readonly IRepository<CouponRedemption> _redemptionRepo;
    private readonly IRepository<Coupon> _couponRepo;

    public UsageController(
        IRepository<UserUsage> usageRepo,
        IRepository<CouponRedemption> redemptionRepo,
        IRepository<Coupon> couponRepo)
    {
        _usageRepo = usageRepo;
        _redemptionRepo = redemptionRepo;
        _couponRepo = couponRepo;
    }

    /// <summary>
    /// Get today's usage for the authenticated user, plus whether they have unlimited access.
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult> GetUsage(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var usage = (await _usageRepo.FindAsync(u => u.UserId == userId && u.Date == today))
            .FirstOrDefault();

        var hasUnlimited = await HasUnlimitedAccess(userId);

        return Ok(new
        {
            chatsUsed = usage?.ChatsUsed ?? 0,
            articlesUsed = usage?.ArticlesUsed ?? 0,
            maxChats = 5,
            maxArticles = 5,
            unlimited = hasUnlimited,
            date = today.ToString("yyyy-MM-dd")
        });
    }

    /// <summary>
    /// Increment chat usage. Returns whether the action is allowed.
    /// </summary>
    [HttpPost("{userId:guid}/chat")]
    public async Task<ActionResult> IncrementChat(Guid userId)
    {
        if (await HasUnlimitedAccess(userId))
        {
            await IncrementUsage(userId, chat: true);
            return Ok(new { allowed = true, unlimited = true });
        }

        var usage = await GetOrCreateUsage(userId);
        if (usage.ChatsUsed >= 5)
        {
            return Ok(new { allowed = false, message = "You've reached your daily limit of 5 chats. Come back tomorrow or enter a coupon code for unlimited access!" });
        }

        usage.ChatsUsed++;
        await _usageRepo.UpdateAsync(usage);
        return Ok(new { allowed = true, chatsUsed = usage.ChatsUsed, remaining = 5 - usage.ChatsUsed });
    }

    /// <summary>
    /// Increment article usage. Returns whether the action is allowed.
    /// </summary>
    [HttpPost("{userId:guid}/article")]
    public async Task<ActionResult> IncrementArticle(Guid userId)
    {
        if (await HasUnlimitedAccess(userId))
        {
            await IncrementUsage(userId, chat: false);
            return Ok(new { allowed = true, unlimited = true });
        }

        var usage = await GetOrCreateUsage(userId);
        if (usage.ArticlesUsed >= 5)
        {
            return Ok(new { allowed = false, message = "You've reached your daily limit of 5 article refreshes. Come back tomorrow or enter a coupon code for unlimited access!" });
        }

        usage.ArticlesUsed++;
        await _usageRepo.UpdateAsync(usage);
        return Ok(new { allowed = true, articlesUsed = usage.ArticlesUsed, remaining = 5 - usage.ArticlesUsed });
    }

    /// <summary>
    /// Redeem a coupon code for unlimited access.
    /// </summary>
    [HttpPost("{userId:guid}/redeem")]
    public async Task<ActionResult> RedeemCoupon(Guid userId, [FromBody] RedeemCouponRequest request)
    {
        var code = request.Code?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code))
            return BadRequest(new { message = "Please enter a coupon code." });

        var coupon = (await _couponRepo.FindAsync(c => c.Code == code && c.IsActive)).FirstOrDefault();
        if (coupon == null)
            return BadRequest(new { message = "Invalid coupon code." });

        if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < DateTime.UtcNow)
            return BadRequest(new { message = "This coupon has expired." });

        if (coupon.MaxRedemptions > 0 && coupon.RedeemedCount >= coupon.MaxRedemptions)
            return BadRequest(new { message = "This coupon has reached its maximum redemptions." });

        // Check if user already redeemed this coupon
        var existing = (await _redemptionRepo.FindAsync(r => r.CouponId == coupon.Id && r.UserId == userId)).FirstOrDefault();
        if (existing != null)
            return BadRequest(new { message = "You have already redeemed this coupon." });

        // Redeem
        var redemption = new CouponRedemption
        {
            Id = Guid.NewGuid(),
            CouponId = coupon.Id,
            UserId = userId,
            RedeemedAt = DateTime.UtcNow,
        };
        await _redemptionRepo.AddAsync(redemption);

        coupon.RedeemedCount++;
        await _couponRepo.UpdateAsync(coupon);

        return Ok(new { message = "Coupon redeemed! You now have unlimited access.", unlimited = true });
    }

    // ── Helpers ──────────────────────────────────────────────

    private async Task<bool> HasUnlimitedAccess(Guid userId)
    {
        var redemptions = await _redemptionRepo.FindAsync(r => r.UserId == userId);
        foreach (var r in redemptions)
        {
            var coupon = (await _couponRepo.FindAsync(c => c.Id == r.CouponId && c.IsActive)).FirstOrDefault();
            if (coupon != null && (!coupon.ExpiryDate.HasValue || coupon.ExpiryDate.Value >= DateTime.UtcNow))
                return true;
        }
        return false;
    }

    private async Task<UserUsage> GetOrCreateUsage(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var usage = (await _usageRepo.FindAsync(u => u.UserId == userId && u.Date == today)).FirstOrDefault();
        if (usage != null) return usage;

        usage = new UserUsage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = today,
            ChatsUsed = 0,
            ArticlesUsed = 0,
        };
        await _usageRepo.AddAsync(usage);
        return usage;
    }

    private async Task IncrementUsage(Guid userId, bool chat)
    {
        var usage = await GetOrCreateUsage(userId);
        if (chat) usage.ChatsUsed++;
        else usage.ArticlesUsed++;
        await _usageRepo.UpdateAsync(usage);
    }
}

public record RedeemCouponRequest(string Code);
