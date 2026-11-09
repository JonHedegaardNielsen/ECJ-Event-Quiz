namespace EventQuiz.Api.Models;

public class Participant
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<QuizSession> QuizSessions { get; set; } = new();
    public List<ParticipantAnswer> Answers { get; set; } = new();
}
