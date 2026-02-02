using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayurveda_AI_Backend.WebAPI.Controllers;

[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;
    private readonly IGeminiService _geminiService;

    public ArticlesController(IArticleService articleService, IGeminiService geminiService)
    {
        _articleService = articleService;
        _geminiService = geminiService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ArticleDto>>> GetAll()
    {
        var articles = await _articleService.GetAllAsync();
        return Ok(articles);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticleDto>> GetById(Guid id)
    {
        var article = await _articleService.GetByIdAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        return Ok(article);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ArticleDto>> Create([FromBody] CreateArticleDto dto)
    {
        var article = await _articleService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = article.Id }, article);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ArticleDto>> Update(Guid id, [FromBody] UpdateArticleDto dto)
    {
        var article = await _articleService.UpdateAsync(id, dto);
        if (article == null)
        {
            return NotFound();
        }

        return Ok(article);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _articleService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("generate")]
    [Authorize]
    public async Task<ActionResult<GenerateArticlesResponseDto>> Generate([FromBody] GenerateArticlesRequestDto request)
    {
        var response = await _geminiService.GenerateArticlesAsync(request);
        return Ok(response);
    }
}
