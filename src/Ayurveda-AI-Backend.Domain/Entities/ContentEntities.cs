using Ayurveda_AI_Backend.Domain.Enums;

namespace Ayurveda_AI_Backend.Domain.Entities;

public class Article
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class QuizQuestion
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
}

public class QuizOption
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string OptionValue { get; set; } = string.Empty;

    public QuizQuestion? Question { get; set; }
}

public class McqResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid QuestionId { get; set; }
    public string AnswerValue { get; set; } = string.Empty;
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public QuizQuestion? Question { get; set; }
}


public class GeminiQuestion
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
