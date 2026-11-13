using ECJ.EventQuiz.Core.Data;
using ECJ.EventQuiz.Core.Hubs;
using ECJ.EventQuiz.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECJ.EventQuiz.ServerUI.Hosting;

/// <summary>
/// Starts/stops Kestrel + SignalR inside the WPF process.
/// Default bind is 0.0.0.0 so localhost, 127.0.0.1, and LAN IP all work.
/// The WPF UI calls <see cref="StartAsync"/> / <see cref="StopAsync"/>.
/// Uses the same SQLite database as the API (<see cref="EventQuizDatabase"/>).
/// </summary>
public sealed class QuizServerHost(string? connectionString = null) : IAsyncDisposable
{
    public const int DefaultPort = 5188;
    public const string HubPath = "/hubs/quiz";

    private WebApplication? _app;

    public bool IsRunning => _app is not null;
    public int Port { get; private set; } = DefaultPort;
    public bool BindAllInterfaces { get; private set; } = true;
    public string BindUrl { get; private set; } = $"http://0.0.0.0:{DefaultPort}";

    public event Action? StatusChanged;

    public async Task StartAsync(int port = DefaultPort, bool bindAllInterfaces = true)
    {
        if (IsRunning)
            return;

        Port = port is < 1 or > 65535 ? DefaultPort : port;
        BindAllInterfaces = bindAllInterfaces;
        BindUrl = bindAllInterfaces
            ? $"http://0.0.0.0:{Port}"
            : $"http://127.0.0.1:{Port}";

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.WebHost.UseUrls(BindUrl);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite(connectionString ?? EventQuizDatabase.DefaultConnectionString));
        builder.Services.AddSingleton<QuizSessionService>();
        builder.Services.AddSignalR();
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        _app = builder.Build();

        using (var db = _app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext())
            EventQuizDatabase.Initialize(db);

        _app.UseCors();
        _app.MapGet("/", () => Results.Ok(new
        {
            service = "ECJ EventQuiz",
            hub = HubPath,
            bind = BindUrl,
            status = "running"
        }));
        _app.MapGet("/api/health", () => Results.Ok(new { ok = true }));
        _app.MapHub<QuizHub>(HubPath);

        _ = _app.RunAsync();
        await Task.Delay(250);
        StatusChanged?.Invoke();
    }

    public async Task StopAsync()
    {
        if (_app is null)
            return;

        try
        {
            await _app.StopAsync();
        }
        catch
        {
            // Best-effort stop from UI.
        }
        finally
        {
            await _app.DisposeAsync();
            _app = null;
            StatusChanged?.Invoke();
        }
    }

    public IReadOnlyList<string> GetClientBaseUrls() =>
        NetworkAddresses.GetClientHosts(BindAllInterfaces)
            .Select(host => $"http://{host}:{Port}")
            .ToList();

    public async ValueTask DisposeAsync() => await StopAsync();
}
