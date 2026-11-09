using System.Text.Json;
using ECJ.EventQuiz.Core.Dtos;
using ECJ.EventQuiz.Tests.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;

namespace ECJ.EventQuiz.Tests.Integration;

/// <summary>
/// Integration tests against a real SignalR hub over Kestrel.
/// Verifies hub methods and broadcast events with HubConnection clients.
/// </summary>
public class QuizHubSignalRTests
{
    [Fact]
    public async Task CreateSession_and_JoinLobby_broadcasts_LobbyUpdated()
    {
        await using var host = new TestSignalRHost();
        await host.StartAsync();

        await using var hostConn = CreateConnection(host.HubUrl);
        await using var playerConn = CreateConnection(host.HubUrl);
        await hostConn.StartAsync();
        await playerConn.StartAsync();

        var lobbyTcs = new TaskCompletionSource<LobbyUpdatedDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        hostConn.On<JsonElement>("LobbyUpdated", payload =>
        {
            var dto = Deserialize<LobbyUpdatedDto>(payload);
            // JoinAsHost also sends LobbyUpdated (often empty) — wait for a real join.
            if (dto.Participants.Count > 0)
                lobbyTcs.TrySetResult(dto);
        });

        var created = await hostConn.InvokeAsync<SessionCreatedDto>("CreateSession", 1);
        await hostConn.InvokeAsync("JoinAsHost", created.RoomId);
        await playerConn.InvokeAsync("JoinLobby", created.RoomId, "Anna");

        var lobby = await lobbyTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(created.RoomId, lobby.RoomId);
        Assert.Contains(lobby.Participants, p => p.Name == "Anna");
    }

    [Fact]
    public async Task StartQuiz_sends_QuizStarted_and_QuestionChanged()
    {
        await using var host = new TestSignalRHost();
        await host.StartAsync();

        await using var hostConn = CreateConnection(host.HubUrl);
        await using var playerConn = CreateConnection(host.HubUrl);
        await hostConn.StartAsync();
        await playerConn.StartAsync();

        var startedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var questionTcs = new TaskCompletionSource<QuestionChangedDto>(TaskCreationOptions.RunContinuationsAsynchronously);

        playerConn.On<object>("QuizStarted", _ => startedTcs.TrySetResult(true));
        playerConn.On<JsonElement>("QuestionChanged", payload =>
            questionTcs.TrySetResult(Deserialize<QuestionChangedDto>(payload)));

        var created = await hostConn.InvokeAsync<SessionCreatedDto>("CreateSession", 1);
        await playerConn.InvokeAsync("JoinLobby", created.RoomId, "Anna");
        await hostConn.InvokeAsync("StartQuiz", created.RoomId);

        Assert.True(await startedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        var question = await questionTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("countdown", question.Phase);
        Assert.Equal(0, question.Question.Index);
    }

    [Fact]
    public async Task SubmitAnswer_broadcasts_AnswerCountsUpdated()
    {
        await using var host = new TestSignalRHost();
        await host.StartAsync();

        await using var hostConn = CreateConnection(host.HubUrl);
        await using var playerConn = CreateConnection(host.HubUrl);
        await hostConn.StartAsync();
        await playerConn.StartAsync();

        var openTcs = new TaskCompletionSource<QuestionChangedDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var countsTcs = new TaskCompletionSource<AnswerCountsUpdatedDto>(TaskCreationOptions.RunContinuationsAsynchronously);

        playerConn.On<JsonElement>("QuestionChanged", payload =>
        {
            var dto = Deserialize<QuestionChangedDto>(payload);
            if (dto.Phase == "question")
                openTcs.TrySetResult(dto);
        });
        hostConn.On<JsonElement>("AnswerCountsUpdated", payload =>
            countsTcs.TrySetResult(Deserialize<AnswerCountsUpdatedDto>(payload)));

        var created = await hostConn.InvokeAsync<SessionCreatedDto>("CreateSession", 1);
        await playerConn.InvokeAsync("JoinLobby", created.RoomId, "Anna");
        await hostConn.InvokeAsync("StartQuiz", created.RoomId);
        await hostConn.InvokeAsync("OpenQuestion", created.RoomId);

        var open = await openTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await playerConn.InvokeAsync("SubmitAnswer", created.RoomId, open.Question.Id, open.Question.Options[0].Id);

        var counts = await countsTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(open.Question.Id, counts.QuestionId);
        Assert.Equal(1, counts.Counts.Sum());
    }

    [Fact]
    public async Task Health_endpoint_returns_ok()
    {
        await using var host = new TestSignalRHost();
        await host.StartAsync();

        using var http = new HttpClient();
        var response = await http.GetAsync($"{host.BaseUrl}/api/health");
        response.EnsureSuccessStatusCode();
    }

    private static HubConnection CreateConnection(string hubUrl) =>
        new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .Build();

    private static T Deserialize<T>(JsonElement element) =>
        JsonSerializer.Deserialize<T>(element.GetRawText(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
}
