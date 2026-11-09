namespace ECJ.EventQuiz.Core.Services;

/// <summary>
/// Calculates points for a single answer.
/// Faster correct answers score higher (1000 max → 100 at timeout).
/// Wrong answers always score 0.
/// </summary>
public static class ScoreCalculator
{
    public static int Calculate(bool isCorrect, double answeredAfterSeconds, int timeLimitSeconds)
    {
        if (!isCorrect || timeLimitSeconds <= 0)
            return 0;

        var clamped = Math.Clamp(answeredAfterSeconds, 0, timeLimitSeconds);
        var remainingRatio = 1.0 - (clamped / timeLimitSeconds);
        return (int)Math.Round(100 + remainingRatio * 900);
    }
}
