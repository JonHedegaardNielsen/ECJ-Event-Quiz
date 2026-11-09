using ECJ.EventQuiz.Core.Dtos;
using ECJ.EventQuiz.Core.Services;
using Microsoft.AspNetCore.SignalR;

namespace ECJ.EventQuiz.Core.Hubs;

/// <summary>
/// SignalR hub for live quiz play.
/// Clients call methods (JoinLobby, StartQuiz, …).
/// The hub broadcasts events (LobbyUpdated, QuestionChanged, …) to the room group.
/// Each roomId maps to SignalR group "room:{roomId}".
/// </summary>
public class QuizHub(QuizSessionService sessions) : Hub
{
    private static string Group(string roomId) => $"room:{roomId}";

    public async Task<SessionCreatedDto> CreateSession(int quizId)
    {
        var created = sessions.CreateSession(quizId, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, Group(created.RoomId));
        return created;
    }

    public async Task JoinAsHost(string roomId)
    {
        sessions.SetHostConnection(roomId, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, Group(roomId));

        var lobby = sessions.GetRequired(roomId);
        await Clients.Caller.SendAsync(
            "LobbyUpdated",
            new LobbyUpdatedDto(
                lobby.RoomId,
                lobby.Participants.Select(p => new LobbyParticipantDto(p.Id, p.Name)).ToList()));
    }

    public async Task JoinLobby(string roomId, string name)
    {
        var updated = sessions.JoinLobby(roomId, name, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, Group(roomId));
        await Clients.Group(Group(roomId)).SendAsync("LobbyUpdated", updated);
    }

    public async Task StartQuiz(string roomId)
    {
        var changed = sessions.StartQuiz(roomId);
        await Clients.Group(Group(roomId)).SendAsync("QuizStarted", new { roomId });
        await Clients.Group(Group(roomId)).SendAsync("QuestionChanged", changed);
    }

    /// <summary>
    /// Opens the answer window after countdown (3-2-1).
    /// </summary>
    public async Task OpenQuestion(string roomId)
    {
        var changed = sessions.BeginQuestion(roomId);
        await Clients.Group(Group(roomId)).SendAsync("QuestionChanged", changed);
    }

    public async Task SubmitAnswer(string roomId, int questionId, int answerOptionId)
    {
        var (_, counts) = sessions.SubmitAnswer(roomId, Context.ConnectionId, questionId, answerOptionId);
        await Clients.Group(Group(roomId)).SendAsync("AnswerCountsUpdated", counts);
    }

    public async Task CloseQuestion(string roomId)
    {
        var reveal = sessions.CloseQuestion(roomId);
        await Clients.Group(Group(roomId)).SendAsync("QuestionClosed", reveal);
    }

    public async Task NextQuestion(string roomId)
    {
        var result = sessions.NextQuestionOrEnd(roomId);
        if (result is QuizEndedDto ended)
        {
            await Clients.Group(Group(roomId)).SendAsync("QuizEnded", ended);
            return;
        }

        await Clients.Group(Group(roomId)).SendAsync("QuestionChanged", result);
    }

    public async Task EndQuiz(string roomId)
    {
        var ended = sessions.EndQuiz(roomId);
        await Clients.Group(Group(roomId)).SendAsync("QuizEnded", ended);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var updated = sessions.RemoveByConnection(Context.ConnectionId);
        if (!string.IsNullOrEmpty(updated.RoomId))
            await Clients.Group(Group(updated.RoomId)).SendAsync("LobbyUpdated", updated);

        await base.OnDisconnectedAsync(exception);
    }
}
