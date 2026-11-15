using ECJ.EventQuiz.Core.Data;
using Microsoft.Data.Sqlite;

namespace ECJ.EventQuiz.Tests.Unit;

/// <summary>
/// Verifies that the live host reads questions from the same SQLite schema as EventQuiz.Api.
/// </summary>
public class SqliteQuizRepositoryTests
{
    [Fact]
    public void Reads_quiz_with_questions_and_options_from_sqlite()
    {
        var path = CreateDatabase();
        try
        {
            var repository = new SqliteQuizRepository(path);

            var quiz = repository.GetQuiz(7);

            Assert.NotNull(quiz);
            Assert.Equal("Fredagsquiz", quiz!.Name);
            Assert.Equal(2, quiz.Questions.Count);

            var first = quiz.Questions[0];
            Assert.Equal("Hvad er 2 + 2?", first.Text);
            Assert.Equal(3, first.AnswerOptions.Count);
            Assert.Equal("4", first.AnswerOptions.Single(o => o.IsCorrect).Answer);

            var second = quiz.Questions[1];
            Assert.Equal("Hvilken farve har himlen?", second.Text);
            Assert.Equal(2, second.AnswerOptions.Count);
            Assert.Equal("Blå", second.AnswerOptions.Single(o => o.IsCorrect).Answer);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(path);
        }
    }

    [Fact]
    public void Unknown_quiz_returns_null()
    {
        var path = CreateDatabase();
        try
        {
            Assert.Null(new SqliteQuizRepository(path).GetQuiz(999));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(path);
        }
    }

    [Fact]
    public void Missing_database_throws_clear_error()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.db");
        var repository = new SqliteQuizRepository(path);

        var error = Assert.Throws<InvalidOperationException>(() => repository.GetQuiz(1));
        Assert.Contains("blev ikke fundet", error.Message);
    }

    private static string CreateDatabase()
    {
        var path = Path.Combine(Path.GetTempPath(), $"eventquiz-{Guid.NewGuid():N}.db");
        using var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE "Quizzes" (
                "ID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT NOT NULL,
                "CreatorId" INTEGER NOT NULL
            );
            CREATE TABLE "Questions" (
                "ID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                "Question" TEXT NOT NULL,
                "QuizId" INTEGER NOT NULL
            );
            CREATE TABLE "AnswerOptions" (
                "ID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                "IsCorrect" INTEGER NOT NULL,
                "Answer" TEXT NOT NULL,
                "QuestionId" INTEGER NOT NULL
            );

            INSERT INTO "Quizzes" ("ID", "Name", "CreatorId") VALUES (7, 'Fredagsquiz', 1);
            INSERT INTO "Questions" ("ID", "Question", "QuizId") VALUES (10, 'Hvad er 2 + 2?', 7);
            INSERT INTO "Questions" ("ID", "Question", "QuizId") VALUES (11, 'Hvilken farve har himlen?', 7);
            INSERT INTO "AnswerOptions" ("IsCorrect", "Answer", "QuestionId") VALUES (0, '3', 10);
            INSERT INTO "AnswerOptions" ("IsCorrect", "Answer", "QuestionId") VALUES (1, '4', 10);
            INSERT INTO "AnswerOptions" ("IsCorrect", "Answer", "QuestionId") VALUES (0, '5', 10);
            INSERT INTO "AnswerOptions" ("IsCorrect", "Answer", "QuestionId") VALUES (1, 'Blå', 11);
            INSERT INTO "AnswerOptions" ("IsCorrect", "Answer", "QuestionId") VALUES (0, 'Rød', 11);
            """;
        command.ExecuteNonQuery();

        return path;
    }
}
