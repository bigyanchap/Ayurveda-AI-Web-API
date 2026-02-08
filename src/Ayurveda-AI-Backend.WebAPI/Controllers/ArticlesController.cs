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

    [HttpPost("generate")]
    [Authorize]
    public async Task<ActionResult<GenerateArticlesResponseDto>> GenerateArticles(
        [FromBody] GenerateArticlesRequestDto request)
    {
        var articles = await _geminiService.GenerateArticlesAsync(request);
        return Ok(articles);
    }
}
