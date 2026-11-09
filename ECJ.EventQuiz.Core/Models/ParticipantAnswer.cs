namespace ECJ.EventQuiz.Core.Models;

/// <summary>
/// One submitted answer from a participant for a specific question.
/// </summary>
public class ParticipantAnswer
{
    public int ParticipantId { get; set; }
    public int QuestionId { get; set; }
    public int AnswerOptionId { get; set; }
    public bool IsCorrect { get; set; }
    public int Points { get; set; }
    public double AnsweredAfterSeconds { get; set; }
}
