using Microsoft.EntityFrameworkCore;

namespace ECJ.EventQuiz.Core.Data;

/// <summary>
/// Shared SQLite database used by both the API and the SignalR server (ServerUI).
/// Dev: next to ECJ.EventQuiz.sln. Installed app: %LocalAppData%\ECJ.EventQuiz\
/// </summary>
public static class EventQuizDatabase
{
    public const string FileName = "eventquiz.db";

    public static string DefaultConnectionString => $"Data Source={DefaultPath}";

    public static string DefaultPath { get; } = FindDefaultPath();

    /// <summary>
    /// Creates/updates the database (migrations) and adds the demo user and test quizzes.
    /// </summary>
    public static void Initialize(AppDbContext db)
    {
        db.Database.Migrate();
        SeedData.EnsureSeeded(db);
    }

    private static string FindDefaultPath()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ECJ.EventQuiz.sln")))
                return Path.Combine(dir.FullName, FileName);
        }

        var dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ECJ.EventQuiz");
        Directory.CreateDirectory(dataDir);
        return Path.Combine(dataDir, FileName);
    }
}
