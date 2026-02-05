using Ayurveda_AI_Backend.Domain.Enums;

namespace Ayurveda_AI_Backend.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public AuthProvider AuthProvider { get; set; }
    public string AuthProviderUserId { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSignInAt { get; set; }

    public UserProfile? Profile { get; set; }
    public ICollection<PrakritiQuizResponse> PrakritiQuizResponses { get; set; } = new List<PrakritiQuizResponse>();
    public ICollection<PrakritiResult> PrakritiResults { get; set; } = new List<PrakritiResult>();
    public ICollection<HealthSignal> HealthSignals { get; set; } = new List<HealthSignal>();
    public ICollection<ChronicCondition> ChronicConditions { get; set; } = new List<ChronicCondition>();
    public ICollection<UserLifestyleProfile> LifestyleProfiles { get; set; } = new List<UserLifestyleProfile>();
    public ICollection<VikritiSnapshot> VikritiSnapshots { get; set; } = new List<VikritiSnapshot>();
    public ICollection<CouponRedemption> CouponRedemptions { get; set; } = new List<CouponRedemption>();
    public ICollection<UserUsage> UserUsages { get; set; } = new List<UserUsage>();
    public ICollection<McqResponse> McqResponses { get; set; } = new List<McqResponse>();
}

public class UserProfile
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; } = Gender.Unknown;
    public DateTime? DateOfBirth { get; set; }
    public decimal? WeightLbs { get; set; }
    public int? HeightFeet { get; set; }
    public int? HeightInches { get; set; }
    public string? Country { get; set; }
    public string? Timezone { get; set; }
    public string? PreferredLanguage { get; set; }

    public User? User { get; set; }
}
