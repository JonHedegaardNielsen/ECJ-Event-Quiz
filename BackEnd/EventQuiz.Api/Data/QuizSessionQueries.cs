using EventQuiz.Api.DTOs;
using ECJ.EventQuiz.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EventQuiz.Api.Data;

public static class QuizSessionQueries
{
    public static Task<QuizSession?> LoadSessionAsync(this AppDbContext db, string roomId) =>
        db.QuizSessions
            .Include(s => s.Quiz)
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.RoomId == roomId);

    public static Task<List<Question>> GetOrderedQuestionsAsync(this AppDbContext db, int quizId) =>
        db.Questions
            .Include(q => q.AnswerOptions)
            .Where(q => q.QuizId == quizId)
            .OrderBy(q => q.Id)
            .ToListAsync();

    public static async Task<PlayerQuestionDto?> GetCurrentPlayerQuestionAsync(this AppDbContext db, QuizSession session)
    {
        if (session.CurrentQuestionId is null) return null;

        var questions = await db.GetOrderedQuestionsAsync(session.QuizId);
        var index = questions.FindIndex(q => q.Id == session.CurrentQuestionId);
        return index < 0 ? null : questions[index].ToPlayerDto(index + 1, questions.Count);
    }

    public static async Task<string> GenerateRoomIdAsync(this AppDbContext db)
    {
        string roomId;
        do
        {
            roomId = Random.Shared.Next(100000, 1000000).ToString();
        } while (await db.QuizSessions.AnyAsync(s => s.RoomId == roomId));
        return roomId;
    }
}
