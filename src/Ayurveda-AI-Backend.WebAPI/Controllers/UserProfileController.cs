using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayurveda_AI_Backend.WebAPI.Controllers;

[ApiController]
[Route("api/users")]
public class UserProfileController : ControllerBase
{
    private readonly IUserService _userService;

    public UserProfileController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{userId:guid}/profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> GetProfile(Guid userId)
    {
        var profile = await _userService.GetProfileAsync(userId);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPut("{userId:guid}/profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> UpsertProfile(Guid userId, [FromBody] UserProfileDto dto)
    {
        if (userId != dto.UserId)
        {
            return BadRequest("UserId mismatch.");
        }

        var profile = await _userService.UpsertProfileAsync(dto);
        return Ok(profile);
    }
}
