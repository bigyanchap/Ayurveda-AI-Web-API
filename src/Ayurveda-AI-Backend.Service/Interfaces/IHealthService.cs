using Ayurveda_AI_Backend.Domain.DTOs;

namespace Ayurveda_AI_Backend.Service.Interfaces;

public interface IHealthService
{
    Task<IReadOnlyList<HealthIndicatorDto>> GetIndicatorsAsync(Guid userId);
    Task<PrakritiResultDto> SavePrakritiResultAsync(PrakritiResultDto dto);
    Task<PrakritiResultDto?> GetPrakritiResultAsync(Guid userId);
    Task<IReadOnlyList<HealthIndicatorDto>> SaveIndicatorsAsync(IReadOnlyList<HealthIndicatorDto> dto);
}
