namespace EventQuiz.Api.Models;

public class QuizSession
{
    public string RoomId { get; set; } = string.Empty;

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public int? CurrentQuestionId { get; set; }
    public Question? CurrentQuestion { get; set; }

    // Many-to-many through the QuizSessionParticipants table.
    public List<Participant> Participants { get; set; } = new();

    public SessionStatus Status { get; set; } = SessionStatus.Lobby;
}
