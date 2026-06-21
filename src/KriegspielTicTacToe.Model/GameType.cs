namespace KriegspielTicTacToe.Model;

/// <summary>
/// Represents a game type configuration including board builders and play mode settings.
/// </summary>
public record struct GameType(
    IEnumerable<BoardBuilder> BoardBuilders,
    bool IsSynchronousMode,
    bool IsRandomPlayerOrder = false  // defaults to false for default constructor, or set true for random order
);
