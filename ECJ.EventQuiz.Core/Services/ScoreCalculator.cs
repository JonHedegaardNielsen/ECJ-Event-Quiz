namespace ECJ.EventQuiz.Core.Services;

/// <summary>
/// Calculates points for a single answer using the Kahoot point system.
/// Correct answers under 0.5 seconds get max points (1000).
/// Otherwise: points = 1000 × (1 − (answer time ÷ time limit) ÷ 2), rounded. Minimum 500 at timeout.
/// Wrong answers always score 0.
/// </summary>
public static class ScoreCalculator
{
    public const int MaxPoints = 1000;
    public const double InstantAnswerSeconds = 0.5;

    public static int Calculate(bool isCorrect, double answeredAfterSeconds, int timeLimitSeconds)
    {
        if (!isCorrect || timeLimitSeconds <= 0)
            return 0;

        if (answeredAfterSeconds < InstantAnswerSeconds)
            return MaxPoints;

        var clamped = Math.Clamp(answeredAfterSeconds, 0, timeLimitSeconds);
        var responseRatio = clamped / timeLimitSeconds;
        return (int)Math.Round(MaxPoints * (1 - responseRatio / 2), MidpointRounding.AwayFromZero);
    }
}
