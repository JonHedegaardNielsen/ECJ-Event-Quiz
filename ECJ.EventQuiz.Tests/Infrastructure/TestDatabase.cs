using ECJ.EventQuiz.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ECJ.EventQuiz.Tests.Infrastructure;

/// <summary>
/// Creates a fresh SQLite test database (temp file) with the seed quizzes.
/// Quiz ids 1, 2 and 3 are Firmafest 2026, Teambuilding and Sommerfest.
/// </summary>
public static class TestDatabase
{
    public static string CreateConnectionString() =>
        $"Data Source={Path.Combine(Path.GetTempPath(), $"eventquiz-test-{Guid.NewGuid():N}.db")}";

    public static IDbContextFactory<AppDbContext> CreateFactory(string? connectionString = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString ?? CreateConnectionString())
            .Options;
        var factory = new PooledDbContextFactory<AppDbContext>(options);

        using var db = factory.CreateDbContext();
        EventQuizDatabase.Initialize(db);
        return factory;
    }
}
