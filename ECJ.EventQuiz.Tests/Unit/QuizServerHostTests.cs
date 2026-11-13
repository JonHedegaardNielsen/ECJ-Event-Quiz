using ECJ.EventQuiz.ServerUI.Hosting;
using ECJ.EventQuiz.Tests.Infrastructure;

namespace ECJ.EventQuiz.Tests.Unit;

/// <summary>
/// Unit/smoke tests for starting and stopping <see cref="ECJ.EventQuiz.ServerUI.Hosting.QuizServerHost"/>.
/// </summary>
public class QuizServerHostTests
{
    [Fact]
    public async Task Start_and_stop_exposes_client_urls()
    {
        var port = 5200 + Random.Shared.Next(0, 400);
        await using var host = new QuizServerHost(TestDatabase.CreateConnectionString());

        await host.StartAsync(port, bindAllInterfaces: false);
        try
        {
            Assert.True(host.IsRunning);
            Assert.Equal(port, host.Port);
            Assert.Contains($"127.0.0.1:{port}", host.BindUrl);
            Assert.Contains(host.GetClientBaseUrls(), u => u.Contains($"localhost:{port}"));
            Assert.Contains(host.GetClientBaseUrls(), u => u.Contains($"127.0.0.1:{port}"));
        }
        finally
        {
            await host.StopAsync();
        }

        Assert.False(host.IsRunning);
    }
}
