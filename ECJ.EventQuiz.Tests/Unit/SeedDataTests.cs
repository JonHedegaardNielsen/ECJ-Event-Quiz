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
                Assert.Equal(1, q.AnswerOptions.Count(o => o.IsCorrect));
                Assert.True(q.TimeLimitSeconds > 0);
            });
        });

        var correctIndexes = quizzes
            .SelectMany(quiz => quiz.Questions)
            .Select(q => q.AnswerOptions.FindIndex(o => o.IsCorrect))
            .ToList();
        Assert.Contains(correctIndexes, i => i > 0);
        Assert.True(correctIndexes.Distinct().Count() >= 3);
    }

    [Fact]
    public void SyncDemoQuizzes_updates_existing_answer_keys()
    {
        using var db = TestDatabase.CreateFactory().CreateDbContext();

        var first = db.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.AnswerOptions)
            .OrderBy(q => q.Id)
            .First();
        var option = first.Questions.OrderBy(q => q.Id).First().AnswerOptions.OrderBy(a => a.Id).First();
        option.IsCorrect = true;
        foreach (var other in first.Questions.OrderBy(q => q.Id).First().AnswerOptions.Skip(1))
            other.IsCorrect = false;
        db.SaveChanges();

        SeedData.SyncDemoQuizzes(db);

        var refreshed = db.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.AnswerOptions)
            .Single(q => q.Name == "Firmafest 2026");
        var q1 = refreshed.Questions.OrderBy(q => q.Id).First();
        var correct = q1.AnswerOptions.Single(o => o.IsCorrect);
        Assert.Equal("Møllehøj", correct.Answer);
        Assert.NotEqual(q1.AnswerOptions.OrderBy(a => a.Id).First().Id, correct.Id);
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
