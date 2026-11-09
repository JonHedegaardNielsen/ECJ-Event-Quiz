namespace ECJ.EventQuiz.Core.Models;

/// <summary>
/// A quiz owned by a user, containing an ordered list of questions.
/// </summary>
public class Quiz
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CreatorId { get; set; }
    public User? Creator { get; set; }
    public List<Question> Questions { get; set; } = [];
}
