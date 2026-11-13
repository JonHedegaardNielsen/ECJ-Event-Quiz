using ECJ.EventQuiz.Core.Data;
using EventQuiz.Api.Data;
using EventQuiz.Api.DTOs;
using ECJ.EventQuiz.Core.Models;
using ECJ.EventQuiz.Core.Dtos;
using ECJ.EventQuiz.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventQuiz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizSessionController(
    AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<QuizSessionDto>> StartSession(StartSessionRequest request)
    {
        var quiz = await db.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == request.QuizId);
        if (quiz is null) return NotFound($"Quizzen med id {request.QuizId} findes ikke.");
        if (quiz.Questions.Count == 0) return BadRequest("Quizzen har ingen spørgsmål.");

        var session = new QuizSession
        {
            RoomId = await db.GenerateRoomIdAsync(),
            QuizId = quiz.Id,
            Quiz = quiz,
            Status = SessionStatus.Lobby
        };

        db.QuizSessions.Add(session);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSession), new { roomId = session.RoomId }, session.ToDto());
    }

    [HttpGet("{roomId}")]
    public async Task<ActionResult<QuizSessionDto>> GetSession(string roomId)
    {
        var session = await db.LoadSessionAsync(roomId);
        return session is null ? NotFound() : session.ToDto();
    }

    [HttpGet("{roomId}/current-question")]
    public async Task<ActionResult<PlayerQuestionDto>> GetCurrentQuestion(string roomId)
    {
        var session = await db.LoadSessionAsync(roomId);
        if (session is null) return NotFound();
        if (session.Status != SessionStatus.Running) return Conflict($"Sessionen har status {session.Status}.");

        var question = await db.GetCurrentPlayerQuestionAsync(session);
        return question is null ? NoContent() : question;
    }

    [HttpGet("{roomId}/leaderboard")]
    public async Task<ActionResult<List<LeaderboardEntryDto>>> GetLeaderboard(string roomId)
    {
        var session = await db.LoadSessionWithScoresAsync(roomId);
        return session is null ? NotFound() : LeaderBoardCalculator.Calculate(session).ToList();
    }

    [HttpPost("{roomId}/end")]
    public async Task<ActionResult<List<LeaderboardEntryDto>>> EndSession(string roomId)
    {
        var session = await db.QuizSessions.FirstOrDefaultAsync(s => s.RoomId == roomId);
        if (session is null) return NotFound();

        session.Status = SessionStatus.Finished;
        session.CurrentQuestionId = null;
        await db.SaveChangesAsync();

        return LeaderBoardCalculator.Calculate((await db.LoadSessionWithScoresAsync(roomId))!).ToList();
    }
}
