using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Ayurveda_AI_Backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayurveda_AI_Backend.WebAPI.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;
    private readonly IRepository<QuizQuestion> _quizRepository;
    private readonly IRepository<GeminiQuestion> _geminiQuestionRepository;

    public HealthController(
        IHealthService healthService,
        IRepository<QuizQuestion> quizRepository,
        IRepository<GeminiQuestion> geminiQuestionRepository)
    {
        _healthService = healthService;
        _quizRepository = quizRepository;
        _geminiQuestionRepository = geminiQuestionRepository;
    }

    [HttpGet("get-health-result/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<PrakritiResultDto>> GetPrakritiResult(Guid userId)
    {
        var result1 = await _healthService.GetPrakritiResultAsync(userId);
        var result2 = await _healthService.GetIndicatorsAsync(userId);
        if (result1 == null && result2 == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            PrakritiResult = result1,
            Indicators = result2
        });
    }

    [HttpPost("post-prakriti")]
    [Authorize]
    public async Task<ActionResult<PrakritiResultDto>> SavePrakriti([FromBody] PrakritiResultDto dto)
    {
        var result = await _healthService.SavePrakritiResultAsync(dto);
        return Ok(result);
    }

    [HttpPost("post-indicators")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<HealthIndicatorDto>>> SaveIndicators(
        [FromBody] IReadOnlyList<HealthIndicatorDto> dto)
    {
        var result = await _healthService.SaveIndicatorsAsync(dto);
        return Ok(result);
    }

    [HttpGet("get-indicators/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<HealthIndicatorDto>>> GetIndicators(Guid userId)
    {
        var indicators = await _healthService.GetIndicatorsAsync(userId);
        return Ok(indicators);
    }

    [HttpGet("quiz-questions")]
    public async Task<ActionResult<IReadOnlyList<QuizQuestion>>> GetQuizQuestions()
    {
        var questions = await _quizRepository.GetAllAsync();
        return Ok(questions);
    }

    [HttpGet("gemini-questions")]
    public async Task<ActionResult<IReadOnlyList<GeminiQuestion>>> GetGeminiQuestions()
    {
        var questions = await _geminiQuestionRepository.GetAllAsync();
        return Ok(questions);
    }
}
