namespace EventQuiz.Api.Models;

public class AnswerOption
{
    public int ID { get; set; }
    public bool IsCorrect { get; set; }
    public string Answer { get; set; } = string.Empty;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}
