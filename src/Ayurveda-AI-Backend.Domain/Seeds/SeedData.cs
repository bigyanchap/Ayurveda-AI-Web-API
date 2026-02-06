using Ayurveda_AI_Backend.Domain.Entities;

namespace Ayurveda_AI_Backend.Domain.Seeds;

public static class SeedData
{
    public static IReadOnlyList<GeminiQuestion> GeminiQuestions => new List<GeminiQuestion>
    {
        new() { Id = Guid.Parse("d7f9c2a1-6b7a-4a2f-9c44-5a2f7f000001"), QuestionText = "How is your digestion today?", Category = "Digestive" },
        new() { Id = Guid.Parse("d7f9c2a1-6b7a-4a2f-9c44-5a2f7f000002"), QuestionText = "How restful was your sleep last night?", Category = "Sleep" },
        new() { Id = Guid.Parse("d7f9c2a1-6b7a-4a2f-9c44-5a2f7f000003"), QuestionText = "What is your current energy level?", Category = "Energy" }
    };
}
