namespace ECJ.EventQuiz.Core.Dtos;

/// <summary>
/// Returned when a host creates a new room.
/// </summary>
public record SessionCreatedDto(string RoomId, int QuizId, string QuizName);

/// <summary>
/// One player shown in the lobby list.
/// </summary>
public record LobbyParticipantDto(int Id, string Name);

/// <summary>
/// Lobby snapshot pushed to everyone in the room.
/// </summary>
public record LobbyUpdatedDto(string RoomId, IReadOnlyList<LobbyParticipantDto> Participants);

/// <summary>
/// Public answer choice (never includes IsCorrect while the question is open).
/// </summary>
public record AnswerOptionDto(int Id, string Answer);

/// <summary>
/// Question payload for countdown/question UI.
/// </summary>
public record QuestionDto(
    int Id,
    string Text,
    int TimeLimitSeconds,
    int Index,
    int Total,
    IReadOnlyList<AnswerOptionDto> Options);

/// <summary>
/// Sent when the current question or live phase changes.
/// Phase is lowercase (countdown, question, etc.).
/// </summary>
public record QuestionChangedDto(string RoomId, string Phase, QuestionDto Question);

/// <summary>
/// Live answer tally for the host view (one count per option, same order as Options).
/// </summary>
public record AnswerCountsUpdatedDto(string RoomId, int QuestionId, IReadOnlyList<int> Counts);

/// <summary>
/// Sent when a question is closed and the correct option is revealed.
/// Leaderboard holds the scores after this question (players read their own points from it).
/// </summary>
public record RevealDto(
    string RoomId,
    int QuestionId,
    int CorrectAnswerOptionId,
    IReadOnlyList<int> Counts,
    IReadOnlyList<LeaderboardEntryDto> Leaderboard);

/// <summary>
/// One row on the final leaderboard.
/// </summary>
public record LeaderboardEntryDto(int ParticipantId, string Name, int Score);

/// <summary>
/// Final quiz result pushed to all clients.
/// </summary>
public record QuizEndedDto(string RoomId, string QuizName, IReadOnlyList<LeaderboardEntryDto> Leaderboard);
