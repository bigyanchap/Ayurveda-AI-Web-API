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
    private readonly IRepository<PrakritiResult> _prakritiRepository;
    private readonly IRepository<VikritiSnapshot> _vikritiRepository;
    private readonly IRepository<HealthSignal> _signalRepository;
    private readonly IRepository<GeminiQuestion> _geminiQuestionRepository;

    public HealthController(
        IHealthService healthService,
        IRepository<QuizQuestion> quizRepository,
        IRepository<PrakritiResult> prakritiRepository,
        IRepository<VikritiSnapshot> vikritiRepository,
        IRepository<HealthSignal> signalRepository,
        IRepository<GeminiQuestion> geminiQuestionRepository)
    {
        _healthService = healthService;
        _quizRepository = quizRepository;
        _prakritiRepository = prakritiRepository;
        _vikritiRepository = vikritiRepository;
        _signalRepository = signalRepository;
        _geminiQuestionRepository = geminiQuestionRepository;
    }

    [HttpPost("signals")]
    [Authorize]
    public async Task<ActionResult<HealthSignalDto>> LogSignal([FromBody] CreateHealthSignalDto dto)
    {
        var result = await _healthService.LogSignalAsync(dto);
        return Ok(result);
    }

    [HttpGet("signals/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<HealthSignalDto>>> GetSignals(Guid userId)
    {
        var result = await _healthService.GetSignalsAsync(userId);
        return Ok(result);
    }

    [HttpPost("vikriti")]
    [Authorize]
    public async Task<ActionResult<VikritiSnapshotDto>> SaveVikriti([FromBody] VikritiSnapshotDto dto)
    {
        var result = await _healthService.SaveVikritiSnapshotAsync(dto);
        return Ok(result);
    }

    [HttpGet("get-prakriti-result/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<PrakritiResultDto>> GetPrakritiResult(Guid userId)
    {
        var result = await _healthService.GetPrakritiResultAsync(userId);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("prakriti")]
    [Authorize]
    public async Task<ActionResult<PrakritiResultDto>> SavePrakriti([FromBody] PrakritiResultDto dto)
    {
        var result = await _healthService.SavePrakritiResultAsync(dto);
        return Ok(result);
    }

    [HttpPost("prakriti/response")]
    [Authorize]
    public async Task<IActionResult> LogPrakritiResponse([FromBody] PrakritiQuizResponseDto dto)
    {
        await _healthService.LogPrakritiResponseAsync(dto);
        return Ok();
    }

    [HttpPost("mcq")]
    [Authorize]
    public async Task<IActionResult> LogMcqResponse([FromBody] McqResponseDto dto)
    {
        await _healthService.LogMcqResponseAsync(dto);
        return Ok();
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

    [HttpGet("analytics/dosha/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<object>> GetDoshaAnalytics(Guid userId)
    {
        var prakriti = (await _prakritiRepository.FindAsync(p => p.UserId == userId && p.IsActive))
            .OrderByDescending(p => p.CalculatedAt)
            .FirstOrDefault();

        var vikriti = (await _vikritiRepository.FindAsync(v => v.UserId == userId))
            .OrderByDescending(v => v.CalculatedAt)
            .FirstOrDefault();

        return Ok(new
        {
            Prakriti = prakriti,
            Vikriti = vikriti
        });
    }

    [HttpGet("analytics/seasonal/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<object>> GetSeasonalSensitivity(Guid userId)
    {
        var signals = await _signalRepository.FindAsync(s => s.UserId == userId);
        var grouped = signals
            .GroupBy(s => s.SignalType)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        return Ok(new
        {
            Season = DateTime.UtcNow.Month,
            SignalCounts = grouped
        });
    }

    [HttpGet("analytics/trends/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<object>> GetTrendCharts(Guid userId)
    {
        var signals = await _signalRepository.FindAsync(s => s.UserId == userId);
        var trendData = signals
            .OrderBy(s => s.ReportedAt)
            .GroupBy(s => s.SignalType)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Select(s => new { s.ReportedAt, s.NumericValue, s.SignalValue }).ToList());

        return Ok(new { Trends = trendData });
    }
}
