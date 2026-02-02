using Ayurveda_AI_Backend.Domain.DTOs;

namespace Ayurveda_AI_Backend.Service.Interfaces;

public interface IGeminiService
{
    Task<ChatResponseDto> GetChatResponseAsync(ChatRequestDto request);
    Task<GenerateArticlesResponseDto> GenerateArticlesAsync(GenerateArticlesRequestDto request);
}
