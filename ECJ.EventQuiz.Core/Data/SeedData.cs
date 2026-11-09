using ECJ.EventQuiz.Core.Models;

namespace ECJ.EventQuiz.Core.Data;

/// <summary>
/// Temporary in-memory quizzes used until Entity Framework/SQLite is ready.
/// Call <see cref="GetQuiz"/> with id 1, 2, or 3 from hub/session code.
/// </summary>
public static class SeedData
{
    public static IReadOnlyList<Quiz> Quizzes { get; } = BuildQuizzes();

    public static Quiz? GetQuiz(int quizId) =>
        Quizzes.FirstOrDefault(q => q.Id == quizId);

    private static List<Quiz> BuildQuizzes()
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
