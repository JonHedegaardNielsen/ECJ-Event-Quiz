using System.Text.Json;
using ECJ.EventQuiz.Core.Dtos;
using ECJ.EventQuiz.Tests.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;

namespace ECJ.EventQuiz.Tests.Acceptance;

/// <summary>
/// End-to-end scenarios from the user's point of view
/// (host + players through a full live quiz over SignalR).
/// </summary>
public class LiveQuizAcceptanceTests
{
    [Fact]
    public async Task Arrangor_and_deltager_can_complete_full_quiz()
    {
        await using var server = new TestSignalRHost();
        await server.StartAsync();

        await using var host = CreateConnection(server.HubUrl);
        await using var player = CreateConnection(server.HubUrl);
        await host.StartAsync();
        await player.StartAsync();

        var playerLobby = WaitFor<LobbyUpdatedDto>(player, "LobbyUpdated");
        var playerStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        player.On<object>("QuizStarted", _ => playerStarted.TrySetResult(true));
        var playerEnded = WaitFor<QuizEndedDto>(player, "QuizEnded");

        var created = await host.InvokeAsync<SessionCreatedDto>("CreateSession", 1);
        Assert.False(string.IsNullOrWhiteSpace(created.RoomId));

        await player.InvokeAsync("JoinLobby", created.RoomId, "Anna");
        var lobby = await playerLobby.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Contains(lobby.Participants, p => p.Name == "Anna");

        await host.InvokeAsync("StartQuiz", created.RoomId);
        await playerStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        const int totalQuestions = 3; // seed quiz 1
        for (var i = 0; i < totalQuestions; i++)
        {
            var openWait = WaitFor<QuestionChangedDto>(
                player,
                "QuestionChanged",
                dto => dto.Phase == "question" && dto.Question.Index == i);

            await host.InvokeAsync("OpenQuestion", created.RoomId);
            var open = await openWait.Task.WaitAsync(TimeSpan.FromSeconds(5));

            await player.InvokeAsync("SubmitAnswer", created.RoomId, open.Question.Id, open.Question.Options[0].Id);

            var closedWait = WaitFor<RevealDto>(player, "QuestionClosed");
            await host.InvokeAsync("CloseQuestion", created.RoomId);
            var closed = await closedWait.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Contains(closed.Leaderboard, e => e.Name == "Anna");

            if (i < totalQuestions - 1)
            {
                var nextWait = WaitFor<QuestionChangedDto>(
                    player,
                    "QuestionChanged",
                    dto => dto.Phase == "countdown" && dto.Question.Index == i + 1);
                await host.InvokeAsync("NextQuestion", created.RoomId);
                await nextWait.Task.WaitAsync(TimeSpan.FromSeconds(5));
            }
            else
            {
                await host.InvokeAsync("NextQuestion", created.RoomId);
            }
        }

        var ended = await playerEnded.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(created.RoomId, ended.RoomId);
        Assert.Equal("Firmafest 2026", ended.QuizName);
        Assert.Contains(ended.Leaderboard, e => e.Name == "Anna");
    }

    [Fact]
    public async Task Two_players_see_same_lobby_and_leaderboard()
    {
        await using var server = new TestSignalRHost();
        await server.StartAsync();

        await using var host = CreateConnection(server.HubUrl);
        await using var p1 = CreateConnection(server.HubUrl);
        await using var p2 = CreateConnection(server.HubUrl);
        await host.StartAsync();
        await p1.StartAsync();
        await p2.StartAsync();

        var hostLobby = WaitFor<LobbyUpdatedDto>(host, "LobbyUpdated", dto => dto.Participants.Count == 2);

        var created = await host.InvokeAsync<SessionCreatedDto>("CreateSession", 2);
        await host.InvokeAsync("JoinAsHost", created.RoomId);
        await p1.InvokeAsync("JoinLobby", created.RoomId, "Anna");
        await p2.InvokeAsync("JoinLobby", created.RoomId, "Bo");

        var lobby = await hostLobby.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(2, lobby.Participants.Count);
        Assert.Contains(lobby.Participants, p => p.Name == "Anna");
        Assert.Contains(lobby.Participants, p => p.Name == "Bo");

        var endedWait = WaitFor<QuizEndedDto>(host, "QuizEnded");
        await host.InvokeAsync("StartQuiz", created.RoomId);

        const int quizQuestionCount = 3;
        for (var i = 0; i < quizQuestionCount; i++)
        {
            await host.InvokeAsync("OpenQuestion", created.RoomId);
            await host.InvokeAsync("CloseQuestion", created.RoomId);
            await host.InvokeAsync("NextQuestion", created.RoomId);
        }

        var ended = await endedWait.Task.WaitAsync(TimeSpan.FromSeconds(8));
        Assert.Equal(2, ended.Leaderboard.Count);
    }

    private static HubConnection CreateConnection(string hubUrl) =>
        new HubConnectionBuilder().WithUrl(hubUrl).Build();

    private static TaskCompletionSource<T> WaitFor<T>(
        HubConnection connection,
        string eventName,
        Func<T, bool>? predicate = null)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<JsonElement>(eventName, payload =>
        {
            var dto = Deserialize<T>(payload);
            if (predicate is null || predicate(dto))
                tcs.TrySetResult(dto);
        });
        return tcs;
    }

    private static T Deserialize<T>(JsonElement element) =>
        JsonSerializer.Deserialize<T>(element.GetRawText(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
}
