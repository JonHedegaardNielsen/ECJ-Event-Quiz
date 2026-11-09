using EventQuiz.Api.Models;

namespace EventQuiz.Api.Services;

public class ScoreCalculator
{
    public const int PointsPerCorrectAnswer = 100;

    public int Calculate(IEnumerable<ParticipantAnswer> answers) =>
        answers.Count(a => a.IsCorrect) * PointsPerCorrectAnswer;
}
