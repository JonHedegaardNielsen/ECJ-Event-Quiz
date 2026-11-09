using EventQuiz.Api.Data;
using EventQuiz.Api.DTOs;
using EventQuiz.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventQuiz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<QuizSummaryDto>>> GetQuizzes([FromQuery] int? creatorId)
    {
        var query = db.Quizzes.AsQueryable();
        if (creatorId is not null) query = query.Where(q => q.CreatorId == creatorId);

        return await query
            .OrderBy(q => q.ID)
            .Select(q => new QuizSummaryDto(q.ID, q.Name, q.CreatorId, q.Questions.Count))
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuizDto>> GetQuiz(int id)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Questions).ThenInclude(q => q.AnswerOptions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(q => q.ID == id);

        return quiz is null ? NotFound() : quiz.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<QuizDto>> CreateQuiz(CreateQuizRequest request)
    {
        if (!await db.Users.AnyAsync(u => u.ID == request.CreatorId))
            return BadRequest($"Brugeren med id {request.CreatorId} findes ikke.");

        if (request.Questions.Any(q => !q.AnswerOptions.Any(a => a.IsCorrect)))
            return BadRequest("Hvert spørgsmål skal have mindst ét korrekt svar.");

        var quiz = new Quiz
        {
            Name = request.Name.Trim(),
            CreatorId = request.CreatorId,
            Questions = request.Questions.Select(q => new Question
            {
                Text = q.Question.Trim(),
                AnswerOptions = q.AnswerOptions.Select(a => new AnswerOption
                {
                    Answer = a.Answer.Trim(),
                    IsCorrect = a.IsCorrect
                }).ToList()
            }).ToList()
        };

        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuiz), new { id = quiz.ID }, quiz.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        var quiz = await db.Quizzes.FindAsync(id);
        if (quiz is null) return NotFound();

        db.Quizzes.Remove(quiz);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
