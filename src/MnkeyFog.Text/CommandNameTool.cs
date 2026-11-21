namespace MnkeyFog.Text;

using System.Text.RegularExpressions;
using OneOf;
using OneOf.Types;

/// <summary>
/// Provides static methods for building player command names and resolving
/// space command names.
/// </summary>
public static class CommandNameTool {
    public static OrderedDictionary<int, string> BuildPlayerToCommandNameMap(IEnumerable<Player> availablePlayers) {
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

    public static string BoardNameFromIndex(int boardIndex)
    => (boardIndex + 1).ToString();

    public static bool TryGetBoardIndexByName(string boardName, int boardsCount, out sbyte boardIndex) {
        var boardNameAsSbyte = sbyte.Parse(boardName);
        boardIndex = (boardNameAsSbyte - 1).AsSByte;
        if (boardIndex >= 0 && boardIndex < boardsCount) {
            return true;
        } else {
            boardIndex = -1;
            return false;
        }
    }

    public static OneOf<NotFound, Result<sbyte>> GetBoardIndexByName(string boardName, int boardsCount) {
        if (TryGetBoardIndexByName(boardName, boardsCount, out var boardIndex)) {
            return new Result<sbyte>(boardIndex);
        } else {
            return new NotFound();
        }
    }

    /// <summary>
    /// For the given space, for the given player, get the textual
    /// representation of the space.  If the game is over, all players see
    /// everything, all marks on the board.  If not, they will only see the ones
    /// they have created or discovered.  If the player is the
    /// current-turn-player, then the space index codes will be displayed.
    /// </summary>
    public static string SpaceCommandName(GameView gameView, sbyte boardIndex, sbyte col, sbyte row) {
        ArgumentNullException.ThrowIfNull(gameView);
        var player = gameView.PlayerIndex;
        player = gameView.IsGameOver //show for all players if the game is over.
            ? null
            : player;

        var boardView = gameView.GetBoardViewByIndex(boardIndex);
        var spaceView = boardView.GetSpaceView(col, row);

        if (
            !spaceView.MarkIndex.HasValue
            && !boardView.IsDone
            && gameView.CanTakeTurn
        ) {
            return boardView.GetSpaceName(gameView, col, row);
        } else {
            return gameView.PlayersState.GetMark(spaceView.MarkIndex);
        }
    }
}
