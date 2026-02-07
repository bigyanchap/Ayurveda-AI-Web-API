using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Enums;

namespace Ayurveda_AI_Backend.Service.Interfaces;

public interface IQuizQuestionService
{
    Task<IReadOnlyList<QuizQuestionDto>> GetAllAsync();
    Task<IReadOnlyList<QuizQuestionDto>> GetByTypeAsync(QuestionType type);
    Task<QuizQuestionDto?> GetByIdAsync(Guid id);
    Task<QuizQuestionDto> CreateAsync(CreateQuizQuestionDto dto);
    Task<QuizQuestionDto?> UpdateAsync(Guid id, UpdateQuizQuestionDto dto);
    Task<bool> DeleteAsync(Guid id);
}
