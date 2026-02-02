using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Domain.Enums;

namespace Ayurveda_AI_Backend.Service.Services;

public static class GeminiPromptBuilder
{
    public static string BuildChatPrompt(
        UserProfile? profile,
        PrakritiResult? prakriti,
        VikritiSnapshot? vikriti,
        IReadOnlyList<HealthSignal> signals,
        string userMessage)
    {
        var timeOfDay = DateTime.Now.Hour switch
        {
            < 6 => "early morning",
            < 12 => "morning",
            < 17 => "afternoon",
            < 21 => "evening",
            _ => "night"
        };

        var month = DateTime.Now.Month;
        var season = month switch
        {
            12 or 1 or 2 => "winter",
            3 or 4 or 5 => "spring",
            6 or 7 or 8 => "summer",
            _ => "autumn"
        };

        var signalSummary = string.Join("; ", signals.Select(s => $"{s.SignalType}:{s.SignalValue ?? s.NumericValue?.ToString()}"));

        return $"""
You are an Ayurvedic health companion. Provide concise, practical guidance.
Time of day: {timeOfDay}
Season: {season}
User profile: {profile?.Gender}, {profile?.Country}, {profile?.PreferredLanguage}
Prakriti: Vata {prakriti?.VataPercent}% Pitta {prakriti?.PittaPercent}% Kapha {prakriti?.KaphaPercent}% Label {prakriti?.PrakritiLabel}
Vikriti: Vata {vikriti?.VataScore} Pitta {vikriti?.PittaScore} Kapha {vikriti?.KaphaScore} Dominant {vikriti?.DominantDosha} Reason {vikriti?.ReasonSummary}
Recent signals: {signalSummary}
User message: {userMessage}
""";
    }

    public static string BuildArticlePrompt(
        UserProfile? profile,
        PrakritiResult? prakriti,
        VikritiSnapshot? vikriti,
        IReadOnlyList<HealthSignal> signals,
        IReadOnlyList<ChronicCondition> conditions,
        UserLifestyleProfile? lifestyle,
        GenerateArticlesRequestDto request)
    {
        var latest = signals
            .OrderByDescending(s => s.ReportedAt)
            .GroupBy(s => s.SignalType)
            .ToDictionary(g => g.Key, g => g.First());

        string? GetSignalValue(SignalType type)
        {
            if (!latest.TryGetValue(type, out var signal))
            {
                return null;
            }

            return signal.SignalValue ?? signal.NumericValue?.ToString();
        }

        int? age = profile?.DateOfBirth is null
            ? null
            : Math.Max(0, (int)((DateTime.UtcNow - profile.DateOfBirth.Value).TotalDays / 365.25));

        var medicalConditions = conditions.Count == 0
            ? "None"
            : string.Join(", ", conditions.Select(c => $"{c.ConditionType} ({c.Severity})"));

        var timeOfDay = string.IsNullOrWhiteSpace(request.TimeOfDay)
            ? "Unknown"
            : request.TimeOfDay;
        var weather = string.IsNullOrWhiteSpace(request.Weather)
            ? "Unknown"
            : request.Weather;
        var location = string.IsNullOrWhiteSpace(request.Location)
            ? profile?.Country ?? "Unknown"
            : request.Location;

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

### **Instructions for content**

1. Articles must be **short and actionable** (100–200 words each).
2. Advice must match **user's dosha and imbalances if available. If not, use what's available and give general advice and also let them know the advice will be better if they provide all the information.**.
3. Include **seasonal adjustments** (Cold/dry morning).
4. Focus on **easily implementable tips**.
5. Avoid generic advice — everything must be **personalized to the user profile above**.
6. Ensure the language is **friendly, clear, and modern**, suitable for a mobile app user.
7. Include **why the recommendation helps** in a single sentence.

---

### **Goal**

Generate 6 personalized Ayurveda articles that the user can **read today and implement immediately**.

### **User Profile & Context (for personalized advice)**:

- Prakriti: Vata {{prakriti?.VataPercent}}% Pitta {{prakriti?.PittaPercent}}% Kapha {{prakriti?.KaphaPercent}}% Label {{prakriti?.PrakritiLabel}}
- Vikriti: Vata {{vikriti?.VataScore}} Pitta {{vikriti?.PittaScore}} Kapha {{vikriti?.KaphaScore}} Dominant {{vikriti?.DominantDosha}} Reason {{vikriti?.ReasonSummary}}
- Discomforts: {{GetSignalValue(SignalType.Discomforts) ?? "Unknown"}}
- DigestionStatus: {{GetSignalValue(SignalType.DigestionStatus) ?? "Unknown"}}
- PoopStatus: {{GetSignalValue(SignalType.Poop) ?? "Unknown"}}
- SleepStatus: {{GetSignalValue(SignalType.Sleep) ?? "Unknown"}}
- MedicalConditions: {{medicalConditions}}
- YogaOrExerciseHours: {{GetSignalValue(SignalType.YogaMins) ?? "Unknown"}}
- NatureOfWork: {{lifestyle?.NatureOfJob ?? GetSignalValue(SignalType.NatureOfJob) ?? "Unknown"}}
- MentalState: {{GetSignalValue(SignalType.MentalState) ?? "Unknown"}}
- ScreenTime: {{GetSignalValue(SignalType.ScreenTime) ?? "Unknown"}}
- HydrationYesterday: {{GetSignalValue(SignalType.HydrationYesterday) ?? "Unknown"}}
- Weight (in pounds): {{profile?.WeightLbs?.ToString() ?? "Unknown"}}
- Height (in feet and inches): {{profile?.HeightFeet?.ToString() ?? "Unknown"}} ft {{profile?.HeightInches?.ToString() ?? "Unknown"}} in
- Age (obtained from D.O.B): {{age?.ToString() ?? "Unknown"}}
- TimeOfDay: {{timeOfDay}}
- Weather: {{weather}}
- Location: {{location}}
""";
    }
}
