using ECJ.EventQuiz.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
    /// Adds the demo user with the test quizzes if missing, and keeps seed-quiz
    /// answer keys in sync (so correct answers are not stuck on option A).
    /// </summary>
    public static void EnsureSeeded(AppDbContext db)
    {
        if (!db.Users.Any(u => u.Username == DemoUsername))
        {
            var demo = new User { Username = DemoUsername };
            demo.PasswordHash = new PasswordHasher<User>().HashPassword(demo, DemoPassword);
            demo.Quizzes.AddRange(CreateQuizzes());
            db.Users.Add(demo);
            db.SaveChanges();
            return;
        }

        SyncDemoQuizzes(db);
    }

    /// <summary>
    /// Replaces questions/options on the known demo quizzes so local databases
    /// pick up corrected answer keys after an update.
    /// </summary>
    public static void SyncDemoQuizzes(AppDbContext db)
    {
        var demo = db.Users
            .Include(u => u.Quizzes)
            .ThenInclude(q => q.Questions)
            .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefault(u => u.Username == DemoUsername);
        if (demo is null)
            return;

        var desired = CreateQuizzes();
        var changed = false;

        foreach (var want in desired)
        {
            var existing = demo.Quizzes.FirstOrDefault(q => q.Name == want.Name);
            if (existing is null)
            {
                demo.Quizzes.Add(want);
                changed = true;
                continue;
            }

            if (QuestionsMatch(existing, want))
                continue;

            if (existing.Questions.Count > 0)
                db.Questions.RemoveRange(existing.Questions);

            existing.Questions = want.Questions
                .Select(q => new Question
                {
                    Text = q.Text,
                    TimeLimitSeconds = q.TimeLimitSeconds,
                    AnswerOptions = q.AnswerOptions
                        .Select(a => new AnswerOption { Answer = a.Answer, IsCorrect = a.IsCorrect })
                        .ToList()
                })
                .ToList();
            changed = true;
        }

        if (changed)
            db.SaveChanges();
    }

    private static bool QuestionsMatch(Quiz existing, Quiz want)
    {
        if (existing.Questions.Count != want.Questions.Count)
            return false;

        for (var i = 0; i < want.Questions.Count; i++)
        {
            var left = existing.Questions.OrderBy(q => q.Id).ElementAt(i);
            var right = want.Questions[i];
            if (!string.Equals(left.Text, right.Text, StringComparison.Ordinal))
                return false;
            if (left.TimeLimitSeconds != right.TimeLimitSeconds)
                return false;
            if (left.AnswerOptions.Count != right.AnswerOptions.Count)
                return false;

            var leftOpts = left.AnswerOptions.OrderBy(a => a.Id).ToList();
            for (var j = 0; j < right.AnswerOptions.Count; j++)
            {
                if (!string.Equals(leftOpts[j].Answer, right.AnswerOptions[j].Answer, StringComparison.Ordinal))
                    return false;
                if (leftOpts[j].IsCorrect != right.AnswerOptions[j].IsCorrect)
                    return false;
            }
        }

        return true;
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

        // correct = indeks 0–3 (A–D). Fordelt, så det rigtige svar ikke altid er A.
        return
        [
            MakeQuiz("Firmafest 2026",
                ("Hvad hedder Danmarks højeste punkt?", ["Yding Skovhøj", "Møllehøj", "Himmelbjerget", "Storebælt"], 1, 20),
                ("Hvilken farve har Danmarks flag?", ["Blå", "Grøn", "Rød", "Gul"], 2, 15),
                ("Hvad er Danmarks nationalfugl?", ["Stær", "Ravn", "Mejse", "Sangsvane"], 3, 20)),
            MakeQuiz("Teambuilding",
                ("Hvor mange ben har en edderkop?", ["6", "10", "8", "4"], 2, 15),
                ("Hvad er den kemiske forkortelse for natrium?", ["K", "Na", "Fe", "N"], 1, 15),
                ("Hvilket organ pumper blodet rundt i kroppen?", ["Leveren", "Milten", "Nyren", "Hjertet"], 3, 15)),
            MakeQuiz("Sommerfest",
                ("Hvad er Danmarks nationalblomst?", ["Anemone", "Marguerit", "Røllike", "Hortensia"], 1, 15),
                ("Hvor mange spillere er på banen pr. fodboldhold?", ["9", "10", "12", "11"], 3, 15),
                ("Hvad hedder Grønlands hovedstad?", ["Ilulissat", "Qaqortoq", "Nuuk", "Tasiilaq"], 2, 15))
        ];
    }
}
