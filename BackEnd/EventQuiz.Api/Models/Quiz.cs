namespace EventQuiz.Api.Models;

public class Quiz
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;

    public int CreatorId { get; set; }
    public User Creator { get; set; } = null!;

    public List<Question> Questions { get; set; } = new();
}
