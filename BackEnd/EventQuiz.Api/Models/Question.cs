namespace EventQuiz.Api.Models;

public class Question
{
    public int ID { get; set; }

    // Mapped to the "Question" column (a C# member cannot share its class name).
    public string Text { get; set; } = string.Empty;

    public List<AnswerOption> AnswerOptions { get; set; } = new();

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
}
