using Ayurveda_AI_Backend.Domain.DTOs;

namespace Ayurveda_AI_Backend.Service.Interfaces;

public interface IUserService
{
    Task<UserProfileDto?> GetProfileAsync(Guid userId);
    Task<UserProfileDto> UpsertProfileAsync(UserProfileDto profile);
}
