namespace MnkeyFog.Text;

using System.Text.RegularExpressions;
using OneOf;
using OneOf.Types;

/// <summary>
/// Names model entities (players, boards, spaces) for the text interface.
/// Unconfigured for now (uses the default naming rules); long-term it will be
/// configured with platform-specific naming conventions.
/// </summary>
public class ModelNamer {
    /// <summary>
    /// The pre-provided board names. Null means the default naming: the
    /// <c>CommandName</c> and <c>DisplayName</c> of board <c>i</c> are both
    /// <c>i + 1</c> (works for any board index).
    /// </summary>
    private readonly IReadOnlyList<ModelName>? _boardModelNames;
    private readonly IReadOnlyList<ModelName>? _columnModelNames;
    private readonly IReadOnlyList<ModelName>? _rowModelNames;

    #region constructors
    /// <summary>
    /// The default namer: board <c>i</c> has <c>CommandName</c> and
    /// <c>DisplayName</c> of <c>i + 1</c> (the current behaviour).
    /// </summary>
    public ModelNamer() {
        _boardModelNames = null;
        _columnModelNames = null;
        _rowModelNames = null;
    }

    /// <summary>
    /// A namer with pre-provided board names. <paramref name="boardModelNames"/>
    /// must have at least as many entries as there are boards, or else
    /// rendering/selecting beyond its length throws.
    /// </summary>
    public ModelNamer(IEnumerable<ModelName> boardModelNames) {
        ArgumentNullException.ThrowIfNull(boardModelNames);
        _boardModelNames = boardModelNames.ToList();
        _columnModelNames = null;
        _rowModelNames = null;
    }

    /// <summary>
    /// A namer with pre-provided board, column, and row names. Each list must
    /// have at least as many entries as there are boards/columns/rows, or else
    /// rendering beyond its length throws.
    /// </summary>
    public ModelNamer(
        IEnumerable<ModelName> boardModelNames,
        IEnumerable<ModelName> columnModelNames,
        IEnumerable<ModelName> rowModelNames) {
        ArgumentNullException.ThrowIfNull(boardModelNames);
        ArgumentNullException.ThrowIfNull(columnModelNames);
        ArgumentNullException.ThrowIfNull(rowModelNames);
        _boardModelNames = boardModelNames.ToList();
        _columnModelNames = columnModelNames.ToList();
        _rowModelNames = rowModelNames.ToList();
    }

    private ModelNamer(IEnumerable<ModelName>? columnModelNames, IEnumerable<ModelName>? rowModelNames) {
        _boardModelNames = null;
        _columnModelNames = columnModelNames?.ToList();
        _rowModelNames = rowModelNames?.ToList();
    }

    /// <summary>
    /// A namer sized for a board of <paramref name="rowCount"/> rows and
    /// <paramref name="columnCount"/> columns. Row names are the 1-based-from-
    /// bottom numbers (<c>"1"</c>..<c>"rowCount"</c>), not zero-padded (unlike the
    /// default namer, so a 10-row board has <c>"9"</c>, not <c>"09"</c>); column
    /// names are the letters <c>"A"</c>..<c>"Z"</c>; board names are derived
    /// (numeric).
    /// </summary>
    public static ModelNamer CreateForBoard(int rowCount, int columnCount) {
        // Row names are stored 0-based from the top; to match the default
        // namer (1-based-from-bottom), the top row is "rowCount" and the bottom
        // row is "1".
        var rowNames = new List<ModelName>();
        for (var i = 0; i < rowCount; i += 1) {
            var name = (rowCount - i).ToString();
            rowNames.Add(new ModelName(name, name));
        }
        var colNames = new List<ModelName>();
        for (var i = 0; i < columnCount; i += 1) {
            var letter = ((char)('A' + i)).ToString();
            colNames.Add(new ModelName(letter, letter));
        }
        return new ModelNamer(colNames, rowNames);
    }
    #endregion

    /// <summary>
    /// The name (<c>CommandName</c> + <c>DisplayName</c>) of the board at
    /// <paramref name="boardIndex"/>. The default namer derives
    /// <c>(boardIndex + 1)</c> for any index; a pre-provided namer looks the
    /// index up in its list, throwing if the index is beyond the list's length.
    /// </summary>
    public ModelName GetBoardModelName(int boardIndex) {
        if (_boardModelNames is null) {
            var name = (boardIndex + 1).ToString();
            return new ModelName(name, name);
        }
        if (boardIndex < 0 || boardIndex >= _boardModelNames.Count) {
            throw new ArgumentOutOfRangeException(nameof(boardIndex), $"Board index {boardIndex} is out of range for the {_boardModelNames.Count} pre-provided board names.");
        }
        return _boardModelNames[boardIndex];
    }

    /// <summary>
    /// The name (<c>CommandName</c> + <c>DisplayName</c>) of the column at
    /// <paramref name="colIndex"/> (0-based from the left). The default namer
    /// derives the letter <c>'A' + colIndex</c>; a pre-provided namer looks the
    /// index up in its list, throwing if beyond the list's length.
    /// </summary>
    public ModelName GetColumnModelName(int colIndex) {
        if (_columnModelNames is null) {
            var name = ((char)('A' + colIndex)).ToString();
            return new ModelName(name, name);
        }
        if (colIndex < 0 || colIndex >= _columnModelNames.Count) {
            throw new ArgumentOutOfRangeException(nameof(colIndex), $"Column index {colIndex} is out of range for the {_columnModelNames.Count} pre-provided column names.");
        }
        return _columnModelNames[colIndex];
    }

    /// <summary>
    /// The name (<c>CommandName</c> + <c>DisplayName</c>) of the row at
    /// <paramref name="rowFromTop"/> (0-based from the top) on a board with
    /// <paramref name="rowCount"/> rows. The default namer derives the
    /// 1-based-from-bottom row number (<c>rowCount - rowFromTop</c>), zero-padded
    /// to the board's width; a pre-provided namer looks <paramref
    /// name="rowFromTop"/> up in its list (0-based from the top), throwing if
    /// beyond the list's length.
    /// </summary>
    public ModelName GetRowModelName(int rowFromTop, int rowCount) {
        if (_rowModelNames is null) {
            var number = rowCount - rowFromTop;
            var width = (int)Math.Log10(rowCount) + 1;
            var name = number.ToString().PadLeft(width, '0');
            return new ModelName(name, name);
        }
        if (rowFromTop < 0 || rowFromTop >= _rowModelNames.Count) {
            throw new ArgumentOutOfRangeException(nameof(rowFromTop), $"Row index {rowFromTop} is out of range for the {_rowModelNames.Count} pre-provided row names.");
        }
        return _rowModelNames[rowFromTop];
    }

    /// <summary>
    /// Builds a mapping of key-commands to players, so that actions that
    /// require choosing a player (like choosing which player is ready in
    /// hotseat mode) can be done with keyboard commands.  If a player's mark is
    /// a typeable character, use that.  If not, assign an alternate unused char
    /// for the player.  Only currently used in `MnkeyFog.CommandLine`.
    /// </summary>
    public OrderedDictionary<int, string> BuildPlayerToCommandNameMap(IEnumerable<Player> availablePlayers) {
        // Build alternate key mapping for ALL players before entering loop
        // Keys are uppercase only (A-Z, 0-9)
        var usedKeys = availablePlayers
            .Select(p => p.Info.Mark)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var playerIndexToKey = new OrderedDictionary<int, string>();

        foreach (var player in availablePlayers) {
            // Check if mark is typeable (ASCII letter or digit)
            bool isTypeable = (player.Info.Mark.Length == 1)
                && Regex.IsMatch(player.Info, "[ABCDEFGHJKLMNOPSTUVWXYZ1-9]", RegexOptions.IgnoreCase); //remove I, R, 0 (zero) and Q.  I and O for confusion, R and Q for command collisions.

            if (isTypeable) {
                // Typeable mark - use the mark itself
                playerIndexToKey[player.Index] = player.Info.Mark;
            } else {
                // Non-typeable mark (emoji, Snowman, etc.) - assign alternate key

                // Try digits first (1-9)
                // Stop if we've exhausted digits (after '9').
                // Start at 1 because "0" looks too much like "O"
                for (int numKeyIndex = 1; numKeyIndex < 10; numKeyIndex += 1) {
                    string digitKey = ((char)('0' + numKeyIndex)).ToString();
                    if (usedKeys.Contains(digitKey)) {
                        continue;
                    } else {
                        playerIndexToKey[player.Index] = digitKey;
                        usedKeys.Add(digitKey);
                        break;
                    }
                }

                if (!playerIndexToKey.ContainsKey(player.Index)) {
                    // If needed, use letters (A-Z)
                    // stop at 26 since then we've exhausted letters.
                    for (int letterKeyIndex = 0; letterKeyIndex < 26; letterKeyIndex += 1) {
                        string letterKey = ((char)('A' + letterKeyIndex)).ToString();
                        if (usedKeys.Contains(letterKey)) {
                            continue;
                        } else {
                            playerIndexToKey[player.Index] = letterKey;
                            usedKeys.Add(letterKey);
                            break;
                        }
                    }
                }
            }
        }

        return playerIndexToKey;
    }

    public bool TryGetBoardIndexByCommandName(string boardName, int boardsCount, out sbyte boardIndex) {
        if (_boardModelNames is null) {
            var boardNameAsSbyte = sbyte.Parse(boardName);
            boardIndex = (boardNameAsSbyte - 1).AsSByte;
            if (boardIndex >= 0 && boardIndex < boardsCount) {
                return true;
            }
            boardIndex = -1;
            return false;
        }
        for (sbyte i = 0; i < boardsCount && i < _boardModelNames.Count; i += 1) {
            if (string.Equals(_boardModelNames[i].CommandName, boardName, StringComparison.OrdinalIgnoreCase)) {
                boardIndex = i;
                return true;
            }
        }
        boardIndex = -1;
        return false;
    }

    public OneOf<NotFound, Result<sbyte>> GetBoardIndexByCommandName(string boardName, int boardsCount) {
        if (TryGetBoardIndexByCommandName(boardName, boardsCount, out var boardIndex)) {
            return new Result<sbyte>(boardIndex);
        } else {
            return new NotFound();
        }
    }

    /// <summary>
    /// The graphical representation of a pickable space: the space's command
    /// name (the text a player types to play the space).  This is the space
    /// name; the decision of *whether* to show it (vs. an occupying mark) is
    /// made by <see cref="GetSpaceBody"/>.
    /// </summary>
    public string SpaceCommandName(GameView gameView, sbyte boardIndex, sbyte col, sbyte row) {
        ArgumentNullException.ThrowIfNull(gameView);
        var boardView = gameView.GetBoardViewByIndex(boardIndex);
        return boardView.GetSpaceName(this, gameView, col, row);
    }

    /// <summary>
    /// The text drawn in a space's body.  When <paramref name="doShowSpaceCommandName"/>
    /// is true and the space is empty on an active board, show the space's
    /// command name (<see cref="SpaceCommandName"/>); otherwise show the
    /// occupying mark.  Fog-of-war filtering is applied upstream by the
    /// <see cref="BoardView"/> (and, once the game is over, the
    /// <see cref="BoardView"/> shows every mark), so this resolves to marks for
    /// all spaces when the game is over.
    /// </summary>
    public string GetSpaceBody(GameView gameView, sbyte boardIndex, sbyte col, sbyte row, bool doShowSpaceCommandName) {
        ArgumentNullException.ThrowIfNull(gameView);
        var boardView = gameView.GetBoardViewByIndex(boardIndex);
        var spaceView = boardView.GetSpaceView(col, row);

        if (
            doShowSpaceCommandName
            && !spaceView.MarkIndex.HasValue
            && !boardView.IsDone
        ) {
            return SpaceCommandName(gameView, boardIndex, col, row);
        } else {
            return gameView.PlayersState.GetMark(spaceView.MarkIndex);
        }
    }
}
