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
    private readonly IReadOnlyList<string>? _playerMarks;

    #region constructors
    /// <summary>
    /// The default namer: board <c>i</c> has <c>CommandName</c> and
    /// <c>DisplayName</c> of <c>i + 1</c> (the current behaviour).
    /// </summary>
    public ModelNamer() {
        _boardModelNames = null;
        _columnModelNames = null;
        _rowModelNames = null;
        _playerMarks = null;
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
        _playerMarks = null;
    }

    /// <summary>
    /// A namer with pre-provided board, column, and row names. Each list must
    /// have at least as many entries as there are boards/columns/rows, or else
    /// rendering beyond its length throws.
    /// </summary>
    public ModelNamer(
        IEnumerable<ModelName> boardModelNames,
        IEnumerable<ModelName> columnModelNames,
        IEnumerable<ModelName> rowModelNames,
        IEnumerable<string>? playerMarks = null
    ) {
        ArgumentNullException.ThrowIfNull(boardModelNames);
        ArgumentNullException.ThrowIfNull(columnModelNames);
        ArgumentNullException.ThrowIfNull(rowModelNames);
        _boardModelNames = boardModelNames.ToList();
        _columnModelNames = columnModelNames.ToList();
        _rowModelNames = rowModelNames.ToList();
        _playerMarks = playerMarks?.ToList();
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
    /// <paramref name="colIndex"/> (0-based from the left) on a board with
    /// <paramref name="columnCount"/> columns. The default namer derives the
    /// base-26 (A=0, B=1, ..., Z=25) letters of <paramref name="colIndex"/>,
    /// left-padded with <c>'A'</c> to the width needed for
    /// <paramref name="columnCount"/> columns (1 letter for up to 26 columns, 2
    /// for up to 676, and so on) — the letter analogue of the default namer's
    /// zero-padded row numbers; a pre-provided namer looks <paramref
    /// name="colIndex"/> up in its list (0-based from the left), throwing if
    /// beyond the list's length.
    /// </summary>
    public ModelName GetColumnModelName(int colIndex, int columnCount) {
        if (_columnModelNames is null) {
            var name = ToAPaddedColumnLetters(colIndex, columnCount);
            return new ModelName(name, name);
        }
        if (colIndex < 0 || colIndex >= _columnModelNames.Count) {
            throw new ArgumentOutOfRangeException(nameof(colIndex), $"Column index {colIndex} is out of range for the {_columnModelNames.Count} pre-provided column names.");
        }
        return _columnModelNames[colIndex];
    }

    /// <summary>
    /// The default (no pre-provided column names) column name for
    /// <paramref name="colIndex"/> (0-based from the left) on a board with
    /// <paramref name="columnCount"/> columns: the base-26 (A=0, B=1, ..., Z=25)
    /// letters of <paramref name="colIndex"/>, left-padded with <c>'A'</c> to the
    /// width needed for <paramref name="columnCount"/> columns.
    /// </summary>
    private static string ToAPaddedColumnLetters(int colIndex, int columnCount) {
        // Width is the number of base-26 digits of the largest column index
        // (columnCount - 1): 1 for up to 26 columns, 2 for up to 676, and so on.
        var width = 1;
        var maxIndex = columnCount - 1;
        while (maxIndex >= 26) {
            maxIndex /= 26;
            width += 1;
        }
        var chars = new char[width];
        for (var i = width - 1; i >= 0; i -= 1) {
            chars[i] = (char)('A' + (colIndex % 26));
            colIndex /= 26;
        }
        return new string(chars);
    }

    /// <summary>
    /// The name (<c>CommandName</c> + <c>DisplayName</c>) of the row at
    /// <paramref name="rowIndex"/> (0-based from the top) on a board with
    /// <paramref name="rowCount"/> rows.
    /// <para>
    /// <paramref name="rowIndex"/> is 0-based from the top (the top row is 0).
    /// For the default namer, the row's <c>CommandName</c> and <c>DisplayName</c>
    /// are the 1-based-from-bottom row number (<c>rowCount - rowIndex</c>),
    /// zero-padded to the width of <paramref name="rowCount"/>; for a
    /// pre-provided namer, <paramref name="rowIndex"/> indexes the list directly
    /// (0-based from the top), throwing if beyond the list's length.
    /// </para>
    /// </summary>
    public ModelName GetRowModelName(int rowIndex, int rowCount) {
        if (_rowModelNames is null) {
            var number = rowCount - rowIndex;
            var width = (int)Math.Log10(rowCount) + 1;
            var name = number.ToString().PadLeft(width, '0');
            return new ModelName(name, name);
        }
        if (rowIndex < 0 || rowIndex >= _rowModelNames.Count) {
            throw new ArgumentOutOfRangeException(nameof(rowIndex), $"Row index {rowIndex} is out of range for the {_rowModelNames.Count} pre-provided row names.");
        }
        return _rowModelNames[rowIndex];
    }

    /// <summary>
    /// The display Mark of the player at <paramref name="playerIndex"/>. If a
    /// pre-provided <c>PlayerMarks</c> list was given to the namer, that entry
    /// is used; otherwise the default: the player's <c>CommandName</c> when it
    /// is a single character, or the <c>playerIndex</c> otherwise. A player's
    /// <c>CommandName</c> (their typeable identity, e.g. a Bluesky handle)
    /// does not apply to the board; the Mark is what is drawn in a space the
    /// player occupies.
    /// </summary>
    public string GetPlayerMark(int playerIndex, PlayersState playersState) {
        if (_playerMarks is not null) {
            if (playerIndex < 0 || playerIndex >= _playerMarks.Count) {
                throw new ArgumentOutOfRangeException(nameof(playerIndex), $"Player index {playerIndex} is out of range for the {_playerMarks.Count} pre-provided player marks.");
            }
            return _playerMarks[playerIndex];
        }
        var commandName = playersState.PlayerInfos[playerIndex].CommandName;
        return commandName.Length == 1 ? commandName : playerIndex.ToString();
    }

    /// <summary>
    /// The combined display name of the player at <paramref name="playerIndex"/>: the Mark alone
    /// when it equals the player's <c>CommandName</c>, otherwise
    /// <c>"CommandName(Mark)"</c>.
    /// </summary>
    public string GetPlayerDisplayName(int playerIndex, PlayersState playersState) {
        var mark = GetPlayerMark(playerIndex, playersState);
        var commandName = playersState.PlayerInfos[playerIndex].CommandName;
        return commandName == mark ? mark : $"{commandName}({mark})";
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
            .Select(p => p.Info.CommandName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var playerIndexToKey = new OrderedDictionary<int, string>();

        foreach (var player in availablePlayers) {
            // Check if mark is typeable (ASCII letter or digit)
            bool isTypeable = (player.Info.CommandName.Length == 1)
                && Regex.IsMatch(player.Info, "[ABCDEFGHJKLMNOPSTUVWXYZ1-9]", RegexOptions.IgnoreCase); //remove I, R, 0 (zero) and Q.  I and O for confusion, R and Q for command collisions.

            if (isTypeable) {
                // Typeable mark - use the mark itself
                playerIndexToKey[player.Index] = player.Info.CommandName;
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
        } else if (spaceView.MarkIndex is null) {
            return Space.EmptyMarkString;
        } else if (spaceView.MarkIndex == Space.ImpasseMarkIndex) {
            return Space.ImpasseChar.ToString();
        } else {
            return GetPlayerMark(spaceView.MarkIndex.Value, gameView.PlayersState);
        }
    }
}
