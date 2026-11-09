using ECJ.EventQuiz.Core.Data;

namespace ECJ.EventQuiz.Tests.Unit;

/// <summary>
/// Unit tests for temporary seed quizzes in <see cref="ECJ.EventQuiz.Core.Data.SeedData"/>.
/// </summary>
public class SeedDataTests
{
    [Fact]
    public void Contains_three_seed_quizzes()
    {
        Assert.Equal(3, SeedData.Quizzes.Count);
        Assert.All(SeedData.Quizzes, q => Assert.False(string.IsNullOrWhiteSpace(q.Name)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void GetQuiz_returns_quiz_with_questions_and_options(int quizId)
    {
        var quiz = SeedData.GetQuiz(quizId);
        Assert.NotNull(quiz);
        Assert.NotEmpty(quiz!.Questions);
        Assert.All(quiz.Questions, q =>
        {
            Assert.NotEmpty(q.AnswerOptions);
            Assert.Contains(q.AnswerOptions, o => o.IsCorrect);
            Assert.True(q.TimeLimitSeconds > 0);
        });
    }

    [Fact]
    public void GetQuiz_unknown_returns_null()
    {
        Assert.Null(SeedData.GetQuiz(999));
    }
}
