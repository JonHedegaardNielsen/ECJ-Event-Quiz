namespace ECJ.EventQuiz.Core.Models;

/// <summary>
/// Detailed live-play phase used by the React UI
/// (lobby → countdown → question → reveal → results).
/// Kept in memory only; not a database enum.
/// </summary>
public enum LivePhase
{
    Lobby,
    Countdown,
    Question,
    Reveal,
    Results
}
