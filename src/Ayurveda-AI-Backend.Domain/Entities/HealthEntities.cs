using Ayurveda_AI_Backend.Domain.Enums;

namespace Ayurveda_AI_Backend.Domain.Entities;

// HealthSignal = "What happened" (time-series data)
public class HealthSignal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public SignalType SignalType { get; set; }
    public string? SignalValue { get; set; }
    public decimal? NumericValue { get; set; }
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "user_input";

    public User? User { get; set; }
}

// HealthIndicator = "How things stand" (current summary)
public class HealthIndicator
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Indication { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
}

public class ChronicCondition
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ConditionType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime? DiagnosedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
}

public class UserLifestyleProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NatureOfJob { get; set; } = string.Empty;
    public string TypicalWorkHours { get; set; } = string.Empty;
    public string PhysicalIntensity { get; set; } = string.Empty;

    public User? User { get; set; }
}

public class VikritiSnapshot
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal VataScore { get; set; }
    public decimal PittaScore { get; set; }
    public decimal KaphaScore { get; set; }
    public DoshaType DominantDosha { get; set; }
    public string ReasonSummary { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}


