using Ayurveda_AI_Backend.Domain.Enums;

namespace Ayurveda_AI_Backend.Domain.Entities;

public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public PlanType PlanType { get; set; }
    public int MaxRedemptions { get; set; }
    public int RedeemedCount { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CouponRedemption> Redemptions { get; set; } = new List<CouponRedemption>();
}

public class CouponRedemption
{
    public Guid Id { get; set; }
    public Guid CouponId { get; set; }
    public Guid UserId { get; set; }
    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

    public Coupon? Coupon { get; set; }
    public User? User { get; set; }
}

public class UserUsage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public int ChatsUsed { get; set; }
    public int ArticlesUsed { get; set; }

    public User? User { get; set; }
}

public class AccessPolicy
{
    public Guid Id { get; set; }
    public PolicyType PolicyType { get; set; }
    public int MaxChatsPerDay { get; set; }
    public int MaxArticlesPerDay { get; set; }
    public bool IsActive { get; set; } = true;
}
