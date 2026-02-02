using Ayurveda_AI_Backend.Domain.Enums;

namespace Ayurveda_AI_Backend.Domain.Entities;

public class PrakritiQuizResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid QuestionId { get; set; }
    public string AnswerValue { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
    public QuizQuestion? Question { get; set; }
}

public class PrakritiResult
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal VataPercent { get; set; }
    public decimal PittaPercent { get; set; }
    public decimal KaphaPercent { get; set; }
    public DoshaType PrakritiLabel { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
