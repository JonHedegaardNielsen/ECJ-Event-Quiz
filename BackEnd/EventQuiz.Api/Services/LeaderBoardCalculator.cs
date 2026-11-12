using EventQuiz.Api.Data;
using EventQuiz.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EventQuiz.Api.Services;

public class LeaderBoardCalculator(AppDbContext db)
{
    public async Task<List<LeaderboardEntryDto>> Calculate(string roomId)
    {
        var session = await db.QuizSessions
            .Include(s => s.Participants)
            .Include(s => s.Answers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.RoomId == roomId);
        if (session is null) return new();

        var ranked = session.Participants
            .Select(p =>
            {
                var own = session.Answers.Where(a => a.ParticipantId == p.Id).ToList();
                return new { Participant = p, Score = own.Sum(a => a.Points), Correct = own.Count(a => a.IsCorrect) };
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Participant.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var result = new List<LeaderboardEntryDto>();
        for (var i = 0; i < ranked.Count; i++)
        {
            // Participants with equal score share the same rank.
            var rank = i > 0 && ranked[i].Score == ranked[i - 1].Score ? result[i - 1].Rank : i + 1;
            result.Add(new LeaderboardEntryDto(rank, ranked[i].Participant.Id, ranked[i].Participant.Name, ranked[i].Score, ranked[i].Correct));
        }
        return result;
    }
}
