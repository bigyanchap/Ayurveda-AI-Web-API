using Ayurveda_AI_Backend.Domain.DTOs;
using Ayurveda_AI_Backend.Domain.Enums;
using Ayurveda_AI_Backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayurveda_AI_Backend.WebAPI.Controllers;

[ApiController]
[Route("api/quiz-questions")]
public class QuizQuestionController : ControllerBase
{
    private readonly IQuizQuestionService _quizQuestionService;

    public QuizQuestionController(IQuizQuestionService quizQuestionService)
    {
        _quizQuestionService = quizQuestionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<QuizQuestionDto>>> GetAll()
    {
        var result = await _quizQuestionService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<IReadOnlyList<QuizQuestionDto>>> GetByType(QuestionType type)
    {
        var result = await _quizQuestionService.GetByTypeAsync(type);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuizQuestionDto>> GetById(Guid id)
    {
        var result = await _quizQuestionService.GetByIdAsync(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<QuizQuestionDto>> Create([FromBody] CreateQuizQuestionDto dto)
    {
        var result = await _quizQuestionService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<QuizQuestionDto>> Update(Guid id, [FromBody] UpdateQuizQuestionDto dto)
    {
        var result = await _quizQuestionService.UpdateAsync(id, dto);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _quizQuestionService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
