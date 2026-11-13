using ECJ.EventQuiz.Core.Services;

namespace ECJ.EventQuiz.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="ECJ.EventQuiz.Core.Services.ScoreCalculator"/> (Kahoot point system).
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

    [Theory]
    [InlineData(0)]
    [InlineData(0.49)]
    [InlineData(-3)]
    public void Correct_answer_under_half_a_second_gives_max_points(double seconds)
    {
        Assert.Equal(1000, ScoreCalculator.Calculate(true, seconds, 20));
    }

    [Fact]
    public void Kahoot_example_2_seconds_of_30_gives_967()
    {
        Assert.Equal(967, ScoreCalculator.Calculate(true, 2, 30));
    }

    [Fact]
    public void Midway_answer_gives_750()
    {
        Assert.Equal(750, ScoreCalculator.Calculate(true, 10, 20));
    }

    [Fact]
    public void Answer_at_timeout_gives_min_points()
    {
        Assert.Equal(500, ScoreCalculator.Calculate(true, 20, 20));
    }

    [Fact]
    public void Elapsed_above_limit_is_clamped_to_min()
    {
        Assert.Equal(500, ScoreCalculator.Calculate(true, 99, 20));
    }
}
