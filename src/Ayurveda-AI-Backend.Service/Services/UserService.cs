using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Repository.Interfaces;
using Ayurveda_AI_Backend.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ayurveda_AI_Backend.Service.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserProfile> _profileRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IRepository<User> userRepository,
        IRepository<UserProfile> profileRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _logger = logger;
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId)
    {
        var profiles = await _profileRepository.FindAsync(p => p.UserId == userId);
        var profile = profiles.FirstOrDefault();
        return profile == null ? null : MapToDto(profile);
    }

    public async Task<UserProfileDto> UpsertProfileAsync(UserProfileDto profile)
    {
        var user = await _userRepository.GetByIdAsync(profile.UserId);
        if (user == null)
        {
            user = new User
            {
                Id = profile.UserId,
                Email = $"unknown-{profile.UserId}@example.local",
                AuthProvider = Domain.Enums.AuthProvider.Email,
                AuthProviderUserId = profile.UserId.ToString(),
                IsEmailVerified = false,
                IsActive = true
            };
            await _userRepository.AddAsync(user);
            _logger.LogInformation("Created placeholder user {UserId}", profile.UserId);
        }

        var existingProfiles = await _profileRepository.FindAsync(p => p.UserId == profile.UserId);
        var existing = existingProfiles.FirstOrDefault();
        if (existing == null)
        {
            existing = new UserProfile { UserId = profile.UserId };
        }

        existing.FirstName = profile.FirstName;
        existing.LastName = profile.LastName;
        existing.Gender = profile.Gender;
        existing.DateOfBirth = profile.DateOfBirth;
        existing.WeightLbs = profile.WeightLbs;
        existing.HeightFeet = profile.HeightFeet;
        existing.HeightInches = profile.HeightInches;
        existing.Country = profile.Country;
        existing.Timezone = profile.Timezone;
        existing.PreferredLanguage = profile.PreferredLanguage;

        if (existing.User == null)
        {
            existing.User = user;
        }

        if (existingProfiles.Count > 0)
        {
            await _profileRepository.UpdateAsync(existing);
            _logger.LogInformation("Updated user profile {UserId}", profile.UserId);
        }
        else
        {
            await _profileRepository.AddAsync(existing);
            _logger.LogInformation("Created user profile {UserId}", profile.UserId);
        }

        return MapToDto(existing);
    }

    private static UserProfileDto MapToDto(UserProfile profile)
    {
        return new UserProfileDto(
            profile.UserId,
            profile.FirstName,
            profile.LastName,
            profile.Gender,
            profile.DateOfBirth,
            profile.WeightLbs,
            profile.HeightFeet,
            profile.HeightInches,
            profile.Country,
            profile.Timezone,
            profile.PreferredLanguage);
    }
}
