namespace ECJ.EventQuiz.Core.Models;

/// <summary>
/// Live quiz room state keyed by <see cref="RoomId"/> (the join code).
/// Until EF is ready, sessions live in memory via <c>QuizSessionService</c>.
/// </summary>
public class QuizSession
{
    public string RoomId { get; set; } = string.Empty;
    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }
    public int? CurrentQuestionId { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Lobby;
    public LivePhase Phase { get; set; } = LivePhase.Lobby;
    public List<Participant> Participants { get; set; } = [];
    public List<ParticipantAnswer> Answers { get; set; } = [];
    public Dictionary<int, int> Scores { get; set; } = [];
    public DateTime? QuestionStartedAtUtc { get; set; }
    public string? HostConnectionId { get; set; }
}
