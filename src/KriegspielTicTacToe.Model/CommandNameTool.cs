namespace KriegspielTicTacToe.Model;

using System.Text.RegularExpressions;
using KriegspielTicTacToe.Model.Views;
using OneOf;
using OneOf.Types;

/// <summary>
/// Static class for functions that calculate a typeable key for a given thing.
/// </summary>
/// <remarks>
/// Because these are functional, they are testable.
/// </remarks>
public static class CommandNameTool {
    public static OrderedDictionary<Player, string> BuildPlayerToCommandNameMap(IEnumerable<Player> availablePlayers) {
        // Build alternate key mapping for ALL players before entering loop
        // Keys are uppercase only (A-Z, 0-9)
        var usedKeys = availablePlayers
            .Select(p => p.Mark)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var playerToKey = new OrderedDictionary<Player, string>();

        foreach (var player in availablePlayers) {
            // Check if mark is typeable (ASCII letter or digit)
            bool isTypeable = (player.Mark.Length == 1)
                && Regex.IsMatch(player.Mark, "[ABCDEFGHJKLMNOPSTUVWXYZ1-9]", RegexOptions.IgnoreCase); //remove I, R, 0 (zero) and Q.  I and O for confusion, R and Q for command collisions.

            if (isTypeable)  {
                // Typeable mark - use the mark itself
                playerToKey[player] = player.Mark;
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
                        playerToKey[player] = digitKey;
                        usedKeys.Add(digitKey);
                        break;
                    }
                }

                if (!playerToKey.ContainsKey(player)) {
                    // If needed, use letters (A-Z)
                    // stop at 26 since then we've exhausted letters.
                    for (int letterKeyIndex = 0; letterKeyIndex < 26; letterKeyIndex += 1) {
                        string letterKey = ((char)('A' + letterKeyIndex)).ToString();
                        if (usedKeys.Contains(letterKey)) {
                            continue;
                        } else {
                            playerToKey[player] = letterKey;
                            usedKeys.Add(letterKey);
                            break;
                        }
                    }
                }
            }
        }

        return playerToKey;
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
        var player = gameView.Player;
        player = gameView.IsGameOver //show for all players if the game is over.
            ? null
            : player;

        var boardView = gameView.GetBoardViewByIndex(boardIndex);
        var spaceView = boardView.GetSpaceView(col, row);

        if (
            string.IsNullOrWhiteSpace(spaceView.Mark) 
            && !boardView.IsDone 
            && gameView.CanTakeTurn
        ) {
            return boardView.GetSpaceName(gameView, col, row);
        } else {
            return spaceView.Mark ?? "";
        }
    }

    public static bool TryGetBoardIndexByName(string boardName, int boardsCount, out sbyte boardIndex) {
        var boardNameAsSbyte = sbyte.Parse(boardName);
        boardIndex = boardNameAsSbyte.Minus1();
        if (boardIndex >= 0 && boardIndex < boardsCount) {
            return true;
        } else {
            boardIndex = -1;
            return false;
        }
    }

    public static string BoardNameFromIndex(int boardIndex)
    => (boardIndex + 1).ToString();

    public static OneOf<NotFound, Result<sbyte>> GetBoardIndexByName(string boardName, int boardsCount) {
        if(TryGetBoardIndexByName(boardName, boardsCount, out var boardIndex)) {
            return new Result<sbyte>(boardIndex);
        } else {
            return new NotFound();
        }
    }
}