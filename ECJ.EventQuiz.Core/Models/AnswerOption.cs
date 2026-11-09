namespace ECJ.EventQuiz.Core.Models;

/// <summary>
/// One possible answer for a question. Exactly one option should be correct.
/// </summary>
public class AnswerOption
{
    public int Id { get; set; }
    public string Answer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
}
