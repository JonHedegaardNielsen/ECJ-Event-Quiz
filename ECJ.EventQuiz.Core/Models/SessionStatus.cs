namespace ECJ.EventQuiz.Core.Models;

/// <summary>
/// Persistent session state (matches the UML/ER diagram).
/// Use this for database status. Use <see cref="LivePhase"/> for detailed live UI steps.
/// </summary>
public enum SessionStatus
{
    Lobby,
    Running,
    Finished
}
