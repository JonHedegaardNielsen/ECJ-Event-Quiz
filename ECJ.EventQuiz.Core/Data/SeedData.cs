using ECJ.EventQuiz.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace ECJ.EventQuiz.Core.Data;

/// <summary>
/// Demo user and test quizzes that are added to the database on startup.
/// Login: <see cref="DemoUsername"/> / <see cref="DemoPassword"/>.
/// </summary>
public static class SeedData
{
    public const string DemoUsername = "demo";
    public const string DemoPassword = "demo123";

    /// <summary>
    /// Adds the demo user with the test quizzes if the demo user does not exist yet.
    /// </summary>
    public static void EnsureSeeded(AppDbContext db)
    {
        if (db.Users.Any(u => u.Username == DemoUsername))
            return;

        var demo = new User { Username = DemoUsername };
        demo.PasswordHash = new PasswordHasher<User>().HashPassword(demo, DemoPassword);
        demo.Quizzes.AddRange(CreateQuizzes());

        db.Users.Add(demo);
        db.SaveChanges();
    }

    public static List<Quiz> CreateQuizzes()
    {
        static Quiz MakeQuiz(string name, params (string text, string[] options, int correct, int time)[] questions) => new()
        {
            Name = name,
            Questions = questions.Select(q => new Question
            {
                Text = q.text,
                TimeLimitSeconds = q.time,
                AnswerOptions = q.options.Select((answer, i) => new AnswerOption
                {
                    Answer = answer,
                    IsCorrect = i == q.correct
                }).ToList()
            }).ToList()
        };

        return
        [
            MakeQuiz("Firmafest 2026",
                ("Hvad hedder Danmarks højeste punkt?", ["Møllehøj", "Yding Skovhøj", "Himmelbjerget", "Storebælt"], 0, 20),
                ("Hvilken farve har Danmarks flag?", ["Rød", "Blå", "Grøn", "Gul"], 0, 15),
                ("Hvad er Danmarks nationalfugl?", ["Sangsvane", "Stær", "Ravn", "Mejse"], 0, 20)),
            MakeQuiz("Teambuilding",
                ("Hvor mange ben har en edderkop?", ["8", "6", "10", "4"], 0, 15),
                ("Hvad er den kemiske forkortelse for natrium?", ["K", "Na", "Fe", "N"], 1, 15),
                ("Hvilket organ pumper blodet rundt i kroppen?", ["Leveren", "Hjertet", "Milten", "Nyren"], 1, 15)),
            MakeQuiz("Sommerfest",
                ("Hvad er Danmarks nationalblomst?", ["Marguerit", "Anemone", "Røllike", "Hortensia"], 0, 15),
                ("Hvor mange spillere er på banen pr. fodboldhold?", ["11", "9", "10", "12"], 0, 15),
                ("Hvad hedder Grønlands hovedstad?", ["Nuuk", "Ilulissat", "Qaqortoq", "Tasiilaq"], 0, 15))
        ];
    }
}

