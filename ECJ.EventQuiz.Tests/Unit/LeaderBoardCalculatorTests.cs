using ECJ.EventQuiz.Core.Models;
using ECJ.EventQuiz.Core.Services;

namespace ECJ.EventQuiz.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="ECJ.EventQuiz.Core.Services.LeaderBoardCalculator"/>.
/// </summary>
public class LeaderBoardCalculatorTests
{
    [Fact]
    public void Orders_by_score_descending_then_name()
    {
        var session = new QuizSession
        {
            Participants =
            [
                new Participant { Id = 1, Name = "Bo" },
                new Participant { Id = 2, Name = "Anna" },
                new Participant { Id = 3, Name = "Chris" }
            ],
            Scores = new Dictionary<int, int>
            {
                [1] = 500,
                [2] = 900,
                [3] = 500
            }
        };

        var board = LeaderBoardCalculator.Calculate(session);

        Assert.Equal(["Anna", "Bo", "Chris"], board.Select(x => x.Name).ToArray());
        Assert.Equal([900, 500, 500], board.Select(x => x.Score).ToArray());
    }

    [Fact]
    public void Missing_score_is_treated_as_zero()
    {
        var session = new QuizSession
        {
            Participants = [new Participant { Id = 7, Name = "Gæst" }]
        };

        var board = LeaderBoardCalculator.Calculate(session);

        Assert.Single(board);
        Assert.Equal(0, board[0].Score);
    }
}
