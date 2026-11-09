using EventQuiz.Api.Data;
using EventQuiz.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EventQuiz.Api.Services;

public class LeaderBoardCalculator(AppDbContext db, ScoreCalculator scoreCalculator)
{
    public async Task<List<LeaderboardEntryDto>> Calculate(string roomId)
    {
        var session = await db.QuizSessions
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.RoomId == roomId);
        if (session is null) return new();

        var participantIds = session.Participants.Select(p => p.ID).ToList();
        var answers = await db.ParticipantAnswers
            .Where(a => participantIds.Contains(a.ParticipantId) && a.Question.QuizId == session.QuizId)
            .ToListAsync();

        var ranked = session.Participants
            .Select(p =>
            {
                var own = answers.Where(a => a.ParticipantId == p.ID).ToList();
                return new { Participant = p, Score = scoreCalculator.Calculate(own), Correct = own.Count(a => a.IsCorrect) };
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Participant.Name)
            .ToList();

        var result = new List<LeaderboardEntryDto>();
        for (var i = 0; i < ranked.Count; i++)
        {
            // Participants with equal score share the same rank.
            var rank = i > 0 && ranked[i].Score == ranked[i - 1].Score ? result[i - 1].Rank : i + 1;
            result.Add(new LeaderboardEntryDto(rank, ranked[i].Participant.ID, ranked[i].Participant.Name, ranked[i].Score, ranked[i].Correct));
        }
        return result;
    }
}
