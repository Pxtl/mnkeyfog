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

    public string BoardNameFromIndex(int boardIndex)
    => (boardIndex + 1).ToString();

    public bool TryGetBoardIndexByName(string boardName, int boardsCount, out sbyte boardIndex) {
        var boardNameAsSbyte = sbyte.Parse(boardName);
        boardIndex = (boardNameAsSbyte - 1).AsSByte;
        if (boardIndex >= 0 && boardIndex < boardsCount) {
            return true;
        } else {
            boardIndex = -1;
            return false;
        }
    }

    public OneOf<NotFound, Result<sbyte>> GetBoardIndexByName(string boardName, int boardsCount) {
        if (TryGetBoardIndexByName(boardName, boardsCount, out var boardIndex)) {
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
