using ECJ.EventQuiz.Core.Data;
using ECJ.EventQuiz.Core.Models;

namespace ECJ.EventQuiz.Tests.Infrastructure;

/// <summary>
/// Fixed quiz content for tests. Production quizzes come from the SQLite database.
/// </summary>
public static class TestQuizzes
{
    public static IReadOnlyList<Quiz> All { get; } = Build();

    public static Quiz? GetQuiz(int quizId) =>
        All.FirstOrDefault(q => q.Id == quizId);

    private static List<Quiz> Build()
    {
        var creator = new User { Id = 1, Username = "arrangor", PasswordHash = "demo" };
        var nextQuestionId = 1;
        var nextOptionId = 1;

        Quiz MakeQuiz(int id, string name, params (string text, string[] options, int correct, int time)[] questions)
        {
            var quiz = new Quiz { Id = id, Name = name, CreatorId = creator.Id, Creator = creator };
            foreach (var (text, options, correct, time) in questions)
            {
                var question = new Question
                {
                    Id = nextQuestionId++,
                    Text = text,
                    TimeLimitSeconds = time,
                    QuizId = quiz.Id,
                    Quiz = quiz
                };

                for (var i = 0; i < options.Length; i++)
                {
                    question.AnswerOptions.Add(new AnswerOption
                    {
                        Id = nextOptionId++,
                        Answer = options[i],
                        IsCorrect = i == correct,
                        QuestionId = question.Id,
                        Question = question
                    });
                }

                quiz.Questions.Add(question);
            }

            return quiz;
        }

        return
        [
            MakeQuiz(1, "Firmafest 2026",
                ("Hvad hedder Danmarks højeste punkt?", ["Møllehøj", "Yding Skovhøj", "Himmelbjerget", "Storebælt"], 0, 20),
                ("Hvilken farve har Danmarks flag?", ["Rød", "Blå", "Grøn", "Gul"], 0, 15),
                ("Hvad er Danmarks nationalfugl?", ["Sangsvane", "Stær", "Ravn", "Mejse"], 0, 20)),
            MakeQuiz(2, "Teambuilding",
                ("Hvor mange ben har en edderkop?", ["8", "6", "10", "4"], 0, 15),
                ("Hvad er den kemiske forkortelse for natrium?", ["K", "Na", "Fe", "N"], 1, 15),
                ("Hvilket organ pumper blodet rundt i kroppen?", ["Leveren", "Hjertet", "Milten", "Nyren"], 1, 15)),
            MakeQuiz(3, "Sommerfest",
                ("Hvad er Danmarks nationalblomst?", ["Marguerit", "Anemone", "Røllike", "Hortensia"], 0, 15),
                ("Hvor mange spillere er på banen pr. fodboldhold?", ["11", "9", "10", "12"], 0, 15),
                ("Hvad hedder Grønlands hovedstad?", ["Nuuk", "Ilulissat", "Qaqortoq", "Tasiilaq"], 0, 15))
        ];
    }
}

/// <summary>
/// Simple in-memory <see cref="IQuizRepository"/> used by unit and integration tests.
/// </summary>
public sealed class InMemoryQuizRepository(IEnumerable<Quiz> quizzes) : IQuizRepository
{
    private readonly Dictionary<int, Quiz> _quizzes = quizzes.ToDictionary(q => q.Id);

    public Quiz? GetQuiz(int quizId) => _quizzes.GetValueOrDefault(quizId);
}
