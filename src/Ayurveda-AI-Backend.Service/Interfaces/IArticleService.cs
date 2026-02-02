using Ayurveda_AI_Backend.Domain.DTOs;

namespace Ayurveda_AI_Backend.Service.Interfaces;

public interface IArticleService
{
    Task<IReadOnlyList<ArticleDto>> GetAllAsync();
    Task<ArticleDto?> GetByIdAsync(Guid id);
    Task<ArticleDto> CreateAsync(CreateArticleDto dto);
    Task<ArticleDto?> UpdateAsync(Guid id, UpdateArticleDto dto);
    Task<bool> DeleteAsync(Guid id);
}
