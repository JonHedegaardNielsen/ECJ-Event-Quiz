namespace ECJ.EventQuiz.Core.Models;

/// <summary>
/// Quiz organizer account.
/// Later filled/persisted by Entity Framework (Mads).
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public List<Quiz> Quizzes { get; set; } = [];
}
