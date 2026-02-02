using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Ayurveda_AI_Backend.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ayurveda_AI_Backend.Service.Services;

public class ArticleService : IArticleService
{
    private readonly IRepository<Article> _articleRepository;
    private readonly ILogger<ArticleService> _logger;

    public ArticleService(IRepository<Article> articleRepository, ILogger<ArticleService> logger)
    {
        _articleRepository = articleRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ArticleDto>> GetAllAsync()
    {
        var articles = await _articleRepository.GetAllAsync();
        return articles.Select(MapToDto).ToList();
    }

    public async Task<ArticleDto?> GetByIdAsync(Guid id)
    {
        var article = await _articleRepository.GetByIdAsync(id);
        return article == null ? null : MapToDto(article);
    }

    public async Task<ArticleDto> CreateAsync(CreateArticleDto dto)
    {
        var article = new Article
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Summary = dto.Summary,
            Content = dto.Content,
            Tags = dto.Tags
        };

        await _articleRepository.AddAsync(article);
        _logger.LogInformation("Article created {ArticleId}", article.Id);
        return MapToDto(article);
    }

    public async Task<ArticleDto?> UpdateAsync(Guid id, UpdateArticleDto dto)
    {
        var article = await _articleRepository.GetByIdAsync(id);
        if (article == null)
        {
            return null;
        }

        article.Title = dto.Title;
        article.Summary = dto.Summary;
        article.Content = dto.Content;
        article.Tags = dto.Tags;
        article.Status = dto.Status;
        article.UpdatedAt = DateTime.UtcNow;

        await _articleRepository.UpdateAsync(article);
        _logger.LogInformation("Article updated {ArticleId}", article.Id);
        return MapToDto(article);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var article = await _articleRepository.GetByIdAsync(id);
        if (article == null)
        {
            return false;
        }

        await _articleRepository.DeleteAsync(article);
        _logger.LogInformation("Article deleted {ArticleId}", article.Id);
        return true;
    }

    private static ArticleDto MapToDto(Article article)
    {
        return new ArticleDto(
            article.Id,
            article.Title,
            article.Summary,
            article.Content,
            article.Tags,
            article.Status,
            article.CreatedAt,
            article.UpdatedAt);
    }
}
