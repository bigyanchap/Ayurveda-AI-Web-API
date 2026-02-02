using Ayurveda_AI_Backend.Domain.Entities;

namespace Ayurveda_AI_Backend.Domain.Seeds;

public static class SeedData
{
    public static IReadOnlyList<PoopType> PoopTypes => new List<PoopType>
    {
        new() { Id = Guid.Parse("f2e2c9a1-4d6b-4abf-9c55-3c2d0e2b1b11"), Name = "Type 1", Description = "Hard lumps, difficult to pass." },
        new() { Id = Guid.Parse("2a0d8b4c-0c6f-4ea1-9f90-4f3c8f4e7b22"), Name = "Type 2", Description = "Sausage-shaped but lumpy." },
        new() { Id = Guid.Parse("c8f5b8e7-6f5b-4c4a-9b90-8e1a0f7d9c33"), Name = "Type 3", Description = "Cracked surface, normal." },
        new() { Id = Guid.Parse("0f9b7d5a-0b78-4b44-8d9a-4e2c2c5f6d44"), Name = "Type 4", Description = "Smooth and soft, ideal." },
        new() { Id = Guid.Parse("a8b9c2d3-3c2d-4f4b-8e7f-2b4c6d8e9f55"), Name = "Type 5", Description = "Soft blobs, clear-cut." }
    };

    public static IReadOnlyList<EnergyLevel> EnergyLevels => new List<EnergyLevel>
    {
        new() { Id = Guid.Parse("b1f0f611-9a8d-4d6b-9b1c-9af1c8f10001"), Name = "Very Low", Description = "Exhausted or drained." },
        new() { Id = Guid.Parse("b1f0f611-9a8d-4d6b-9b1c-9af1c8f10002"), Name = "Low", Description = "Below usual energy." },
        new() { Id = Guid.Parse("b1f0f611-9a8d-4d6b-9b1c-9af1c8f10003"), Name = "Moderate", Description = "Stable energy." },
        new() { Id = Guid.Parse("b1f0f611-9a8d-4d6b-9b1c-9af1c8f10004"), Name = "High", Description = "Energetic and focused." }
    };

    public static IReadOnlyList<HealthIndicator> Indicators => new List<HealthIndicator>
    {
        new() { Id = Guid.Parse("6a6165af-2a69-4c6a-b8d1-4d8f9aa30101"), Name = "Digestion", Description = "Bloating, appetite, regularity.", Category = "Digestive" },
        new() { Id = Guid.Parse("6a6165af-2a69-4c6a-b8d1-4d8f9aa30102"), Name = "Sleep Quality", Description = "Restful sleep and duration.", Category = "Sleep" },
        new() { Id = Guid.Parse("6a6165af-2a69-4c6a-b8d1-4d8f9aa30103"), Name = "Stress", Description = "Mental tension and calmness.", Category = "Mind" },
        new() { Id = Guid.Parse("6a6165af-2a69-4c6a-b8d1-4d8f9aa30104"), Name = "Energy", Description = "Daily vitality.", Category = "Energy" }
    };

    public static IReadOnlyList<GeminiQuestion> GeminiQuestions => new List<GeminiQuestion>
    {
        new() { Id = Guid.Parse("d7f9c2a1-6b7a-4a2f-9c44-5a2f7f000001"), QuestionText = "How is your digestion today?", Category = "Digestive" },
        new() { Id = Guid.Parse("d7f9c2a1-6b7a-4a2f-9c44-5a2f7f000002"), QuestionText = "How restful was your sleep last night?", Category = "Sleep" },
        new() { Id = Guid.Parse("d7f9c2a1-6b7a-4a2f-9c44-5a2f7f000003"), QuestionText = "What is your current energy level?", Category = "Energy" }
    };
}
