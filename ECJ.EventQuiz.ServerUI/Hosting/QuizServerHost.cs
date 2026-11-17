using System.IO;
using System.Text.Json.Serialization;
using ECJ.EventQuiz.Core.Data;
using ECJ.EventQuiz.Core.Hubs;
using ECJ.EventQuiz.Core.Models;
using ECJ.EventQuiz.Core.Services;
using EventQuiz.Api.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace ECJ.EventQuiz.ServerUI.Hosting;

/// <summary>
/// Starts/stops Kestrel inside the WPF process with:
/// REST API (/api), SignalR hub (/hubs/quiz), and the React SPA (wwwroot/app).
/// Default bind is 0.0.0.0 so localhost, 127.0.0.1, and LAN IP all work.
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
            ContentRootPath = AppContext.BaseDirectory,
            WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "app")
        });

        builder.WebHost.UseUrls(BindUrl);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

        var sqlite = connectionString ?? EventQuizDatabase.DefaultConnectionString;
        builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(sqlite));
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(sqlite));
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddSingleton<QuizSessionService>();
        builder.Services.AddSignalR(o => o.AddFilter<QuizHubErrorFilter>());
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(AuthController).Assembly)
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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

        var spaRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot", "app");
        if (Directory.Exists(spaRoot) && File.Exists(Path.Combine(spaRoot, "index.html")))
        {
            var files = new PhysicalFileProvider(spaRoot);
            _app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = files });
            _app.UseStaticFiles(new StaticFileOptions { FileProvider = files });
        }

        _app.MapGet("/api/health", () => Results.Ok(new { ok = true }));
        _app.MapGet("/api/server-info", () => Results.Ok(new
        {
            service = "ECJ EventQuiz",
            hub = HubPath,
            bind = BindUrl,
            status = "running"
        }));
        _app.MapControllers();
        _app.MapHub<QuizHub>(HubPath);

        if (Directory.Exists(spaRoot) && File.Exists(Path.Combine(spaRoot, "index.html")))
        {
            var files = new PhysicalFileProvider(spaRoot);
            _app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = files });
        }

        _ = _app.RunAsync();
        await Task.Delay(250);
        StatusChanged?.Invoke();
    }

    public async Task StopAsync()
    {
        if (_app is null)
            return;

        // Tag appen lokalt med det samme, så UI'en viser "Stoppet" uden at vente på åbne SignalR-forbindelser.
        var app = _app;
        _app = null;
        StatusChanged?.Invoke();

        try
        {
            // Hård timeout: ellers kan StopAsync hænge, mens browsere stadig holder /hubs/quiz åben.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await app.StopAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Timeout – dispose nedenfor lukker resten.
        }
        catch
        {
            // Best-effort stop from UI.
        }
        finally
        {
            try
            {
                await app.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
                // Ignore dispose races after forced stop.
            }

            StatusChanged?.Invoke();
        }
    }

    public IReadOnlyList<string> GetClientBaseUrls() =>
        NetworkAddresses.GetClientHosts(BindAllInterfaces)
            .Select(host => $"http://{host}:{Port}")
            .ToList();

    public async ValueTask DisposeAsync() => await StopAsync();
}
