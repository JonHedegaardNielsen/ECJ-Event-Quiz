using ECJ.EventQuiz.Core.Data;
using ECJ.EventQuiz.Core.Models;
using ECJ.EventQuiz.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECJ.EventQuiz.Tests.Unit;

/// <summary>
/// Unit tests for the demo user and test quizzes in <see cref="ECJ.EventQuiz.Core.Data.SeedData"/>.
/// </summary>
public class SeedDataTests
{
    [Fact]
    public void CreateQuizzes_returns_three_quizzes_with_questions_and_options()
    {
        var quizzes = SeedData.CreateQuizzes();

        Assert.Equal(3, quizzes.Count);
        Assert.All(quizzes, quiz =>
        {
            Assert.False(string.IsNullOrWhiteSpace(quiz.Name));
            Assert.NotEmpty(quiz.Questions);
            Assert.All(quiz.Questions, q =>
            {
                Assert.NotEmpty(q.AnswerOptions);
                Assert.Contains(q.AnswerOptions, o => o.IsCorrect);
                Assert.True(q.TimeLimitSeconds > 0);
            });
        });
    }

    [Fact]
    public void Database_contains_demo_user_with_test_quizzes()
    {
        using var db = TestDatabase.CreateFactory().CreateDbContext();

        var demo = db.Users.Include(u => u.Quizzes).ThenInclude(q => q.Questions).Single(u => u.Username == SeedData.DemoUsername);

        Assert.Equal(["Firmafest 2026", "Teambuilding", "Sommerfest"], demo.Quizzes.OrderBy(q => q.Id).Select(q => q.Name));
        Assert.All(demo.Quizzes, q => Assert.Equal(3, q.Questions.Count));
        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            new PasswordHasher<User>().VerifyHashedPassword(demo, demo.PasswordHash, SeedData.DemoPassword));
    }

    [Fact]
    public void EnsureSeeded_does_not_add_quizzes_twice()
    {
        using var db = TestDatabase.CreateFactory().CreateDbContext();

        SeedData.EnsureSeeded(db);

        Assert.Equal(1, db.Users.Count());
        Assert.Equal(3, db.Quizzes.Count());
    }
}
