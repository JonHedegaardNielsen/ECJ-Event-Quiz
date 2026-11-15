using ECJ.EventQuiz.Core.Models;
using Microsoft.Data.Sqlite;

namespace ECJ.EventQuiz.Core.Data;

/// <summary>
/// Reads quizzes from the EventQuiz.Api SQLite database.
/// The live host only needs read access; quiz CRUD stays in EventQuiz.Api.
/// </summary>
public sealed class SqliteQuizRepository : IQuizRepository
{
    private const int DefaultTimeLimitSeconds = 20;

    private readonly string _databasePath;

    public SqliteQuizRepository(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
            throw new ArgumentException("A database path is required.", nameof(databasePath));

        _databasePath = Path.GetFullPath(databasePath);
    }

    public string DatabasePath => _databasePath;

    public Quiz? GetQuiz(int quizId)
    {
        if (!File.Exists(_databasePath))
            throw new InvalidOperationException(
                $"Quizdatabasen blev ikke fundet: {_databasePath}. Start EventQuiz.Api først, eller sæt ECJ_QUIZ_DB.");

        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWrite,
            DefaultTimeout = 5
        }.ToString());
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT q."ID", q."Name", q."CreatorId",
                   qu."ID", qu."Question",
                   ao."ID", ao."Answer", ao."IsCorrect"
            FROM "Quizzes" q
            JOIN "Questions" qu ON qu."QuizId" = q."ID"
            JOIN "AnswerOptions" ao ON ao."QuestionId" = qu."ID"
            WHERE q."ID" = $quizId
            ORDER BY qu."ID", ao."ID";
            """;
        command.Parameters.AddWithValue("$quizId", quizId);

        Quiz? quiz = null;
        var questions = new Dictionary<int, Question>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            quiz ??= new Quiz
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                CreatorId = reader.GetInt32(2)
            };

            var questionId = reader.GetInt32(3);
            if (!questions.TryGetValue(questionId, out var question))
            {
                question = new Question
                {
                    Id = questionId,
                    Text = reader.GetString(4),
                    TimeLimitSeconds = DefaultTimeLimitSeconds,
                    QuizId = quiz.Id,
                    Quiz = quiz
                };
                questions[questionId] = question;
                quiz.Questions.Add(question);
            }

            question.AnswerOptions.Add(new AnswerOption
            {
                Id = reader.GetInt32(5),
                Answer = reader.GetString(6),
                IsCorrect = reader.GetBoolean(7),
                QuestionId = question.Id,
                Question = question
            });
        }

        return quiz;
    }
}
