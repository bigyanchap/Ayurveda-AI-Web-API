using Ayurveda_AI_Backend.Domain.Entities;

namespace Ayurveda_AI_Backend.Service.Services;

public static class GeminiPromptBuilder
{
    /// <summary>
    /// Build the system instruction for multi-turn chat.
    /// Contains the AI persona, user health profile, and current context.
    /// The user message is NOT included — it goes in the Gemini contents array.
    /// </summary>
    public static string BuildChatSystemPrompt(
        UserProfile? profile,
        PrakritiResult? prakriti,
        IReadOnlyList<HealthIndicator> indicators,
        string timeOfDay,
        string weather,
        string location)
    {
        var indicatorBlock = FormatIndicators(indicators);
        var bodyBlock = FormatBodyMetrics(indicators);

        return $"""
You are an Ayurvedic health companion.
Provide concise, practical, and personalized lifestyle guidance rooted in classical Ayurveda.
Always tailor your response using the user's Prakriti, HealthIndicators, Time of Day, Weather, and Location when available.
If any required information is missing, offer safe general guidance and gently encourage the user to complete their profile for more accurate recommendations. Do this at the end of your response.
Use educational and preventive framing only — no diagnosis, treatment, or medical claims.
When using Sanskrit terms, always include a clear English translation in parentheses on first use.
Maintain a calm, respectful, non-judgmental tone.

### User Health Profile
- Prakriti: {FormatPrakriti(prakriti)}
{indicatorBlock}
{bodyBlock}
- Gender: {FormatGender(profile, indicators)}

### Current Context
- Time of Day: {timeOfDay}
- Weather: {weather}
- Location: {location}
""";
    }

    /// <summary>
    /// [Legacy] Single-shot chat prompt (system prompt + user message in one string).
    /// </summary>
    public static string BuildChatPrompt(
        UserProfile? profile,
        PrakritiResult? prakriti,
        IReadOnlyList<HealthIndicator> indicators,
        string timeOfDay,
        string weather,
        string location,
        string userMessage)
    {
        return BuildChatSystemPrompt(profile, prakriti, indicators, timeOfDay, weather, location)
            + $"\n### User Message\n{userMessage}";
    }

    /// <summary>
    /// Build a personalized article generation prompt using the same health params.
    /// </summary>
    public static string BuildArticlePrompt(
        UserProfile? profile,
        PrakritiResult? prakriti,
        IReadOnlyList<HealthIndicator> indicators,
        string timeOfDay,
        string weather,
        string location)
    {
        var indicatorBlock = FormatIndicators(indicators);
        var bodyBlock = FormatBodyMetrics(indicators);

        return $$"""
You are an expert Ayurveda health coach and content writer.
Generate **6 concise, practical, and personalized articles** in the following categories:

1. Food
2. Lifestyle
3. Yoga
4. Recipe
5. Drinks
6. Ayurvedic Herbs / Home Remedies

Output must be in **JSON format**, exactly like this:

[
  { "category": "Food", "title": "string", "content": "string" },
  { "category": "Lifestyle", "title": "string", "content": "string" },
  { "category": "Yoga", "title": "string", "content": "string" },
  { "category": "Recipe", "title": "string", "content": "string" },
  { "category": "Drinks", "title": "string", "content": "string" },
  { "category": "Herbs", "title": "string", "content": "string" }
]

---

### Instructions for content

1. Articles must be **short and actionable** (100–200 words each).
2. Advice must match the **user's dosha and health indicators**. If data is missing, give general advice and mention that personalization improves with a completed profile.
3. Include **weather and location-based adjustments**.
4. Focus on **easily implementable tips**.
5. Avoid generic advice — everything must be **personalized to the user profile below**.
6. Ensure the language is **friendly, clear, and modern**, suitable for a mobile app user.
7. Include **why the recommendation helps** in a single sentence.
8. Dietary Constraint when recommending food: - Strictly Vegetarian-only, that is, No meat, fish, eggs, or seafood.
9. If protein or nourishment is needed: Prefer lentils, legumes, dairy (if suitable), nuts, seeds, and Ayurvedic plant sources.
10. Sanskrit terms must be accompanied by an English translation in parentheses.
---

### Goal

Generate 6 personalized Ayurveda articles that the user can **read today and implement immediately**.

### User Health Profile & Context

- Prakriti: {{FormatPrakriti(prakriti)}}
{{FormatIndicators(indicators)}}
{{FormatBodyMetrics(indicators)}}
- Gender: {{FormatGender(profile, indicators)}}
- Time of Day: {{timeOfDay}}
- Weather: {{weather}}
- Location: {{location}}
""";
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private static string FormatPrakriti(PrakritiResult? prakriti)
    {
        if (prakriti == null)
            return "Not determined yet";

        return $"Vata {prakriti.VataPercent}% | Pitta {prakriti.PittaPercent}% | Kapha {prakriti.KaphaPercent}% (Type: {prakriti.PrakritiLabel})";
    }

    /// <summary>
    /// Resolve gender from UserProfile first, then fall back to HealthIndicator "Gender".
    /// </summary>
    private static string FormatGender(UserProfile? profile, IReadOnlyList<HealthIndicator> indicators)
    {
        // UserProfile.Gender is the authoritative source
        if (profile != null && profile.Gender != Domain.Enums.Gender.Unknown)
            return profile.Gender.ToString();

        // Fall back to HealthIndicator "Gender" (set during onboarding quiz)
        var genderIndicator = indicators.FirstOrDefault(i => i.Indication == "Gender")?.Value;
        return string.IsNullOrWhiteSpace(genderIndicator) ? "Unknown" : genderIndicator;
    }

    /// <summary>
    /// Format HealthIndicator values into a readable block.
    /// </summary>
    private static string FormatIndicators(IReadOnlyList<HealthIndicator> indicators)
    {
        string Get(string indication) =>
            indicators.FirstOrDefault(i => i.Indication == indication)?.Value ?? "Unknown";

        return $"""
- Digestion: {Get("Digestion")}
- Sleep Quality: {Get("SleepQuality")}
- Working Out (minutes/day): {Get("WorkingOutMinutes")}
- Nature of Work: {Get("NatureOfWork")}
- Screen Time: {Get("ScreenTime")}
- Chronic Conditions: {Get("ChronicConditions")}
- Nutrition Deficiency: {Get("NutritionDeficiency")}
""";
    }

    /// <summary>
    /// Extract Age, Weight, and Height from HealthIndicator records.
    /// </summary>
    private static string FormatBodyMetrics(IReadOnlyList<HealthIndicator> indicators)
    {
        string Get(string indication) =>
            indicators.FirstOrDefault(i => i.Indication == indication)?.Value ?? "";

        var dobStr = Get("DateOfBirth");
        var weightStr = Get("Weight");
        var heightStr = Get("Height");

        string ageDisplay = "Unknown";
        if (!string.IsNullOrWhiteSpace(dobStr) && DateTime.TryParse(dobStr, out var dob))
        {
            var age = (int)((DateTime.UtcNow - dob).TotalDays / 365.25);
            ageDisplay = age.ToString();
        }

        var weightDisplay = string.IsNullOrWhiteSpace(weightStr) ? "Unknown" : $"{weightStr} kg";
        var heightDisplay = string.IsNullOrWhiteSpace(heightStr) ? "Unknown" : $"{heightStr} cm";

        return $"""
- Age: {ageDisplay}
- Weight: {weightDisplay}
- Height: {heightDisplay}
""";
    }
}
