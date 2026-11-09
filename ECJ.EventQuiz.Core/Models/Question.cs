namespace ECJ.EventQuiz.Core.Models;

/// <summary>
/// One quiz question with answer options and a time limit in seconds.
/// </summary>
public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; } = 20;
    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }
    public List<AnswerOption> AnswerOptions { get; set; } = [];
}
