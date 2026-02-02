using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayurveda_AI_Backend.WebAPI.Controllers;

[ApiController]
[Route("api/access")]
public class AccessController : ControllerBase
{
    private readonly IRepository<AccessPolicy> _policyRepository;
    private readonly IRepository<UserUsage> _usageRepository;
    private readonly IRepository<Coupon> _couponRepository;
    private readonly IRepository<CouponRedemption> _redemptionRepository;

    public AccessController(
        IRepository<AccessPolicy> policyRepository,
        IRepository<UserUsage> usageRepository,
        IRepository<Coupon> couponRepository,
        IRepository<CouponRedemption> redemptionRepository)
    {
        _policyRepository = policyRepository;
        _usageRepository = usageRepository;
        _couponRepository = couponRepository;
        _redemptionRepository = redemptionRepository;
    }

    [HttpGet("policies")]
    public async Task<ActionResult<IReadOnlyList<AccessPolicy>>> GetPolicies()
    {
        var policies = await _policyRepository.GetAllAsync();
        return Ok(policies);
    }

    [HttpPost("usage")]
    [Authorize]
    public async Task<ActionResult<UserUsageDto>> UpsertUsage([FromBody] UserUsageDto dto)
    {
        var existing = (await _usageRepository.FindAsync(u => u.UserId == dto.UserId && u.Date == dto.Date))
            .FirstOrDefault();

        if (existing == null)
        {
            existing = new UserUsage
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Date = dto.Date,
                ChatsUsed = dto.ChatsUsed,
                ArticlesUsed = dto.ArticlesUsed
            };
            await _usageRepository.AddAsync(existing);
        }
        else
        {
            existing.ChatsUsed = dto.ChatsUsed;
            existing.ArticlesUsed = dto.ArticlesUsed;
            await _usageRepository.UpdateAsync(existing);
        }

        return Ok(dto);
    }

    [HttpPost("coupons/redeem")]
    [Authorize]
    public async Task<ActionResult> RedeemCoupon([FromBody] CouponRedemptionDto dto)
    {
        var coupon = (await _couponRepository.FindAsync(c => c.Id == dto.CouponId && c.IsActive))
            .FirstOrDefault();

        if (coupon == null || (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < DateTime.UtcNow))
        {
            return BadRequest("Coupon is invalid or expired.");
        }

        if (coupon.RedeemedCount >= coupon.MaxRedemptions)
        {
            return BadRequest("Coupon redemption limit reached.");
        }

        coupon.RedeemedCount += 1;
        await _couponRepository.UpdateAsync(coupon);

        var redemption = new CouponRedemption
        {
            Id = Guid.NewGuid(),
            CouponId = dto.CouponId,
            UserId = dto.UserId
        };

        await _redemptionRepository.AddAsync(redemption);
        return Ok();
    }
}
