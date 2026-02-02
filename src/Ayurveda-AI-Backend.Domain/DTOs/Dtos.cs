using Ayurveda_AI_Backend.Domain.Enums;

namespace Ayurveda_AI_Backend.Domain.DTOs;

public record ChatRequestDto(Guid UserId, string Message);
public record ChatResponseDto(string Response, DateTime RespondedAt);

public record ArticleDto(
    Guid Id,
    string Title,
    string Summary,
    string Content,
    string Tags,
    ArticleStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateArticleDto(string Title, string Summary, string Content, string Tags);
public record UpdateArticleDto(string Title, string Summary, string Content, string Tags, ArticleStatus Status);

public record UserProfileDto(
    Guid UserId,
    string FirstName,
    string LastName,
    Gender Gender,
    DateTime? DateOfBirth,
    decimal? WeightLbs,
    int? HeightFeet,
    int? HeightInches,
    string? Country,
    string? Timezone,
    string? PreferredLanguage);

public record HealthSignalDto(
    Guid Id,
    Guid UserId,
    SignalType SignalType,
    string? SignalValue,
    decimal? NumericValue,
    DateTime ReportedAt,
    string Source);

public record CreateHealthSignalDto(
    Guid UserId,
    SignalType SignalType,
    string? SignalValue,
    decimal? NumericValue,
    DateTime? ReportedAt,
    string? Source);

public record PrakritiQuizResponseDto(Guid UserId, Guid QuestionId, string AnswerValue);
public record PrakritiResultDto(Guid UserId, decimal VataPercent, decimal PittaPercent, decimal KaphaPercent, DoshaType PrakritiLabel);
public record VikritiSnapshotDto(Guid UserId, decimal VataScore, decimal PittaScore, decimal KaphaScore, DoshaType DominantDosha, string ReasonSummary);

public record McqResponseDto(Guid UserId, Guid QuestionId, string AnswerValue);

public record CouponDto(Guid Id, string Code, PlanType PlanType, int MaxRedemptions, int RedeemedCount, DateTime? ExpiryDate, bool IsActive);
public record CouponRedemptionDto(Guid CouponId, Guid UserId);
public record UserUsageDto(Guid UserId, DateOnly Date, int ChatsUsed, int ArticlesUsed);
public record AccessPolicyDto(Guid Id, PolicyType PolicyType, int MaxChatsPerDay, int MaxArticlesPerDay, bool IsActive);

public record GenerateArticlesRequestDto(
    Guid UserId,
    string TimeOfDay,
    string Weather,
    string? Location);

public record GenerateArticlesResponseDto(string ArticlesJson, DateTime GeneratedAt);
