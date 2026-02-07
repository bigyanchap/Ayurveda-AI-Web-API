using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Ayurveda_AI_Backend.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ayurveda_AI_Backend.Service.Services;

public class HealthService : IHealthService
{
    private readonly IRepository<HealthSignal> _signalRepository;
    private readonly IRepository<HealthIndicator> _indicatorRepository;
    private readonly IRepository<VikritiSnapshot> _vikritiRepository;
    private readonly IRepository<PrakritiResult> _prakritiRepository;
    private readonly IRepository<PrakritiQuizResponse> _prakritiResponseRepository;
    private readonly IRepository<McqResponse> _mcqRepository;
    private readonly ILogger<HealthService> _logger;

    public HealthService(
        IRepository<HealthSignal> signalRepository,
        IRepository<HealthIndicator> indicatorRepository,
        IRepository<VikritiSnapshot> vikritiRepository,
        IRepository<PrakritiResult> prakritiRepository,
        IRepository<PrakritiQuizResponse> prakritiResponseRepository,
        IRepository<McqResponse> mcqRepository,
        ILogger<HealthService> logger)
    {
        _signalRepository = signalRepository;
        _indicatorRepository = indicatorRepository;
        _vikritiRepository = vikritiRepository;
        _prakritiRepository = prakritiRepository;
        _prakritiResponseRepository = prakritiResponseRepository;
        _mcqRepository = mcqRepository;
        _logger = logger;
    }

    public async Task<HealthSignalDto> LogSignalAsync(CreateHealthSignalDto dto)
    {
        var signal = new HealthSignal
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            SignalType = dto.SignalType,
            SignalValue = dto.SignalValue,
            NumericValue = dto.NumericValue,
            ReportedAt = dto.ReportedAt ?? DateTime.UtcNow,
            Source = dto.Source ?? "user_input"
        };

        await _signalRepository.AddAsync(signal);
        _logger.LogInformation("Health signal logged for user {UserId}", dto.UserId);
        return MapToDto(signal);
    }

    public async Task<IReadOnlyList<HealthSignalDto>> GetSignalsAsync(Guid userId)
    {
        var signals = await _signalRepository.FindAsync(s => s.UserId == userId);
        return signals.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<HealthIndicatorDto>> GetIndicatorsAsync(Guid userId)
    {
        var indicators = await _indicatorRepository.FindAsync(i => i.IsActive && i.UserId == userId);
        _logger.LogInformation("Retrieved {Count} health indicators for user {UserId}", indicators.Count, userId);
        return indicators.Select(i => new HealthIndicatorDto(
            i.Id,
            i.UserId,
            i.Indication,
            i.Value,
            i.IsActive)).ToList();
    }

    public async Task<VikritiSnapshotDto> SaveVikritiSnapshotAsync(VikritiSnapshotDto dto)
    {
        var snapshot = new VikritiSnapshot
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            VataScore = dto.VataScore,
            PittaScore = dto.PittaScore,
            KaphaScore = dto.KaphaScore,
            DominantDosha = dto.DominantDosha,
            ReasonSummary = dto.ReasonSummary
        };

        await _vikritiRepository.AddAsync(snapshot);
        _logger.LogInformation("Vikriti snapshot saved for user {UserId}", dto.UserId);
        return dto;
    }

    public async Task<PrakritiResultDto> SavePrakritiResultAsync(PrakritiResultDto dto)
    {
        var result = new PrakritiResult
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            VataPercent = dto.VataPercent,
            PittaPercent = dto.PittaPercent,
            KaphaPercent = dto.KaphaPercent,
            PrakritiLabel = dto.PrakritiLabel,
            IsActive = true
        };

        await _prakritiRepository.AddAsync(result);
        _logger.LogInformation("Prakriti result saved for user {UserId}", dto.UserId);
        return dto;
    }

    // We never delete or update the indicators, we only set IsActive to false and create new ones. 
    // The Fundamental idea is to never delete Health data, we only set IsActive to false and create new ones.
    public async Task<IReadOnlyList<HealthIndicatorDto>> SaveIndicatorsAsync(IReadOnlyList<HealthIndicatorDto> healthIndicators)
    {
        if (healthIndicators.Count == 0)
            return [];

        var userId = healthIndicators[0].UserId;

        // Deactivate ALL active indicators for this user
        var oldIndicators = await _indicatorRepository.FindAsync(i => i.UserId == userId && i.IsActive);
        foreach (var old in oldIndicators)
        {
            old.IsActive = false;
            await _indicatorRepository.UpdateAsync(old);
        }

        // Save new indicators and set IsActive to true.
        // If any indicator value is empty, carry over from the old active set.
        var newIndicators = healthIndicators.Select(indicator =>
        {
            var value = string.IsNullOrEmpty(indicator.Value)
                ? oldIndicators.FirstOrDefault(o => o.Indication == indicator.Indication)?.Value ?? string.Empty
                : indicator.Value;

            return new HealthIndicator
            {
                Id = Guid.NewGuid(),
                UserId = indicator.UserId,
                Indication = indicator.Indication,
                Value = value,
                IsActive = true,
                CalculatedAt = DateTime.UtcNow
            };
        }).ToList();

        await _indicatorRepository.AddRangeAsync(newIndicators);

        _logger.LogInformation("Indicators saved for user {UserId}", userId);

        return await GetIndicatorsAsync(userId);
    }

    public async Task<PrakritiResultDto?> GetPrakritiResultAsync(Guid userId)
    {
        var result = (await _prakritiRepository.FindAsync(p => p.UserId == userId && p.IsActive))
            .OrderByDescending(p => p.CalculatedAt)
            .FirstOrDefault();

        if (result == null)
        {
            return null;
        }

        return new PrakritiResultDto(
            result.UserId,
            result.VataPercent,
            result.PittaPercent,
            result.KaphaPercent,
            result.PrakritiLabel);
    }

    public async Task LogPrakritiResponseAsync(PrakritiQuizResponseDto dto)
    {
        var response = new PrakritiQuizResponse
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            QuestionId = dto.QuestionId,
            AnswerValue = dto.AnswerValue,
            IsActive = true
        };

        await _prakritiResponseRepository.AddAsync(response);
        _logger.LogInformation("Prakriti response logged for user {UserId}", dto.UserId);
    }

    public async Task LogMcqResponseAsync(McqResponseDto dto)
    {
        var response = new McqResponse
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            QuestionId = dto.QuestionId,
            AnswerValue = dto.AnswerValue
        };

        await _mcqRepository.AddAsync(response);
        _logger.LogInformation("MCQ response logged for user {UserId}", dto.UserId);
    }

    private static HealthSignalDto MapToDto(HealthSignal signal)
    {
        return new HealthSignalDto(
            signal.Id,
            signal.UserId,
            signal.SignalType,
            signal.SignalValue,
            signal.NumericValue,
            signal.ReportedAt,
            signal.Source);
    }
}
