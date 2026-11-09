using ECJ.EventQuiz.Core.Services;

namespace ECJ.EventQuiz.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="ECJ.EventQuiz.Core.Services.ScoreCalculator"/>.
/// </summary>
public class ScoreCalculatorTests
{
    [Fact]
    public void Incorrect_answer_gives_zero()
    {
        Assert.Equal(0, ScoreCalculator.Calculate(false, 1, 20));
    }

    [Fact]
    public void Invalid_time_limit_gives_zero()
    {
        Assert.Equal(0, ScoreCalculator.Calculate(true, 1, 0));
        Assert.Equal(0, ScoreCalculator.Calculate(true, 1, -5));
    }

    [Fact]
    public void Instant_correct_answer_gives_max_points()
    {
        Assert.Equal(1000, ScoreCalculator.Calculate(true, 0, 20));
    }

    [Fact]
    public void Answer_at_timeout_gives_min_points()
    {
        Assert.Equal(100, ScoreCalculator.Calculate(true, 20, 20));
    }

    [Fact]
    public void Midway_answer_is_between_min_and_max()
    {
        var points = ScoreCalculator.Calculate(true, 10, 20);
        Assert.InRange(points, 101, 999);
        Assert.Equal(550, points);
    }

    [Fact]
    public void Negative_elapsed_is_clamped_to_max()
    {
        Assert.Equal(1000, ScoreCalculator.Calculate(true, -3, 20));
    }

    [Fact]
    public void Elapsed_above_limit_is_clamped_to_min()
    {
        Assert.Equal(100, ScoreCalculator.Calculate(true, 99, 20));
    }
}
