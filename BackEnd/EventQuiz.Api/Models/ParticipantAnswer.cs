namespace EventQuiz.Api.Models;

public class ParticipantAnswer
{
    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public int AnswerOptionId { get; set; }
    public AnswerOption AnswerOption { get; set; } = null!;

    public bool IsCorrect { get; set; }
}
