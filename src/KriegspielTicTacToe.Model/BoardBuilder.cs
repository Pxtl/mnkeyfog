using OneOf.Types;

namespace KriegspielTicTacToe.Model;

/// <summary>
/// Parameters to create a board.  If "ScoringLength" is null, that means
/// scoring lines will be expected to go from end-to-end on the board.
/// </summary>
public record struct BoardBuilder(sbyte Width, sbyte Height, sbyte? ScoringLength = null, bool IsBoardDoneWhenScored = false);
