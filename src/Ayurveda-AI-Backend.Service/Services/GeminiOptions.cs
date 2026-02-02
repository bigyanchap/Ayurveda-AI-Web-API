namespace Ayurveda_AI_Backend.Service.Services;

public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3";
    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models";
}
