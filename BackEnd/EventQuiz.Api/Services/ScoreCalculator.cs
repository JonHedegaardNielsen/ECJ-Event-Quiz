namespace EventQuiz.Api.Services;

// Kahoot point system: faster correct answers give more points.
public static class ScoreCalculator
{
    public const int MaxPoints = 1000;
    public const double InstantAnswerSeconds = 0.5;

    public static int Calculate(bool isCorrect, double answeredAfterSeconds, int timeLimitSeconds, int pointsPossible = MaxPoints)
    {
        if (!isCorrect || timeLimitSeconds <= 0) return 0;
        if (answeredAfterSeconds < InstantAnswerSeconds) return pointsPossible;

        var responseRatio = Math.Clamp(answeredAfterSeconds, 0, timeLimitSeconds) / timeLimitSeconds;
        return (int)Math.Round(pointsPossible * (1 - responseRatio / 2), MidpointRounding.AwayFromZero);
    }
}
