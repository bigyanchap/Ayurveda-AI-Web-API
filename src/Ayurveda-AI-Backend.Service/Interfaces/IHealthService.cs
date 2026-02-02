using Ayurveda_AI_Backend.Domain.DTOs;

namespace Ayurveda_AI_Backend.Service.Interfaces;

public interface IHealthService
{
    Task<HealthSignalDto> LogSignalAsync(CreateHealthSignalDto dto);
    Task<IReadOnlyList<HealthSignalDto>> GetSignalsAsync(Guid userId);
    Task<VikritiSnapshotDto> SaveVikritiSnapshotAsync(VikritiSnapshotDto dto);
    Task<PrakritiResultDto> SavePrakritiResultAsync(PrakritiResultDto dto);
    Task LogPrakritiResponseAsync(PrakritiQuizResponseDto dto);
    Task LogMcqResponseAsync(McqResponseDto dto);
}
