using ECJ.EventQuiz.Core.Dtos;
using ECJ.EventQuiz.Core.Models;

namespace ECJ.EventQuiz.Core.Services;

/// <summary>
/// Builds a sorted leaderboard from a session's participants and scores.
/// Sort: highest score first, then name A→Z.
/// </summary>
public static class LeaderBoardCalculator
{
    public static IReadOnlyList<LeaderboardEntryDto> Calculate(QuizSession session)
    {
        return session.Participants
            .Select(p => new LeaderboardEntryDto(
                p.Id,
                p.Name,
                session.Scores.GetValueOrDefault(p.Id)))
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
