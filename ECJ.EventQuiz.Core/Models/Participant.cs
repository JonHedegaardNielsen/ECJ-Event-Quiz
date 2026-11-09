namespace ECJ.EventQuiz.Core.Models;

/// <summary>
/// A player in a live room.
/// <see cref="ConnectionId"/> is SignalR-only runtime state (not a future EF column).
/// </summary>
public class Participant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ConnectionId { get; set; }
}
