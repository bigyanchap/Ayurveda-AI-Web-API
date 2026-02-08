using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayurveda_AI_Backend.WebAPI.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IGeminiService _geminiService;

    public ChatController(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ChatResponseDto>> Chat([FromBody] ChatRequestDto request)
    {
        try
        {
            var response = await _geminiService.GetChatResponseAsync(request);
            return Ok(response);
        }
        catch (Exception)
        {
            return Ok(new ChatResponseDto(
                "I'm having trouble generating a response right now. Please try again in a moment.",
                DateTime.UtcNow));
        }
    }
}
