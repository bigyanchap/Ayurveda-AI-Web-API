using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Domain.Enums;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Ayurveda_AI_Backend.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ayurveda_AI_Backend.Service.Services;

public class QuizQuestionService : IQuizQuestionService
{
    private readonly IRepository<QuizQuestion> _repository;
    private readonly ILogger<QuizQuestionService> _logger;

    public QuizQuestionService(IRepository<QuizQuestion> repository, ILogger<QuizQuestionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<QuizQuestionDto>> GetAllAsync()
    {
        var questions = await _repository.GetAllAsync();
        return questions.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<QuizQuestionDto>> GetByTypeAsync(QuestionType type)
    {
        var questions = await _repository.FindAsync(q => q.QuestionType == type && q.IsActive);
        return questions.Select(MapToDto).ToList();
    }

    public async Task<QuizQuestionDto?> GetByIdAsync(Guid id)
    {
        var question = await _repository.GetByIdAsync(id);
        return question == null ? null : MapToDto(question);
    }

    public async Task<QuizQuestionDto> CreateAsync(CreateQuizQuestionDto dto)
    {
        var question = new QuizQuestion
        {
            Id = Guid.NewGuid(),
            Category = dto.Category,
            QuestionText = dto.QuestionText,
            QuestionType = dto.QuestionType,
            IsActive = true
        };

        await _repository.AddAsync(question);
        _logger.LogInformation("Quiz question created: {Id} of type {Type}", question.Id, dto.QuestionType);
        return MapToDto(question);
    }

    public async Task<QuizQuestionDto?> UpdateAsync(Guid id, UpdateQuizQuestionDto dto)
    {
        var question = await _repository.GetByIdAsync(id);
        if (question == null)
            return null;

        question.Category = dto.Category;
        question.QuestionText = dto.QuestionText;
        question.QuestionType = dto.QuestionType;
        question.IsActive = dto.IsActive;

        await _repository.UpdateAsync(question);
        _logger.LogInformation("Quiz question updated: {Id}", id);
        return MapToDto(question);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var question = await _repository.GetByIdAsync(id);
        if (question == null)
            return false;

        question.IsActive = false;
        await _repository.UpdateAsync(question);
        _logger.LogInformation("Quiz question soft-deleted: {Id}", id);
        return true;
    }

    private static QuizQuestionDto MapToDto(QuizQuestion q) =>
        new(q.Id, q.Category, q.QuestionText, q.QuestionType, q.IsActive);
}
