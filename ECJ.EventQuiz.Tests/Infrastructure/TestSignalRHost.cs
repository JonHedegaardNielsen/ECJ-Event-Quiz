using ECJ.EventQuiz.Core.Hubs;
using ECJ.EventQuiz.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ECJ.EventQuiz.Tests.Infrastructure;

/// <summary>
/// Lightweight Kestrel + SignalR host for tests (no WPF).
/// Mirrors ServerUI hosting so hub tests run against a real endpoint.
/// Use: await using var host = new TestSignalRHost(); await host.StartAsync();
/// </summary>
public sealed class TestSignalRHost : IAsyncDisposable
{
    private WebApplication? _app;

    public string BaseUrl { get; private set; } = string.Empty;
    public string HubUrl => $"{BaseUrl}/hubs/quiz";

    public async Task StartAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton<QuizSessionService>();
        builder.Services.AddSignalR();
        builder.Services.AddCors(o =>
            o.AddDefaultPolicy(p => p.SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        _app = builder.Build();
        _app.UseCors();
        _app.MapGet("/api/health", () => Results.Ok(new { ok = true }));
        _app.MapHub<QuizHub>("/hubs/quiz");

        await _app.StartAsync();

        var addresses = _app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()
            ?.Addresses
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Could not resolve server address.");

        BaseUrl = addresses.TrimEnd('/');
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is null)
            return;

        await _app.StopAsync();
        await _app.DisposeAsync();
        _app = null;
    }
}
