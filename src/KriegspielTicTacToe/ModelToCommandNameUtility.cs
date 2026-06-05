namespace KriegspielTicTacToe;

using System.Text.RegularExpressions;
using KriegspielTicTacToe.Model;

/// <summary>
/// Static class for functions that calculate a typeable key for a given thing.
/// </summary>
/// <remarks>
/// Because these are functional, they are testable.
/// </remarks>
public static class ModelToCommandNameUtility {
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
                && Regex.IsMatch(player.Mark, "[a-zA-Z0-9]");

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

        return playerToKey;
    }


    /// <summary>
    /// For the given space, for the given player, get the textual
    /// representation of the space.  If the game is over, all players see
    /// everything, all marks on the board.  If not, they will only see the ones
    /// they have created or discovered.  If the player is the
    /// current-turn-player, then the space index codes will be displayed.
    /// </summary>
    public static string GetSpaceCommandName(TicTacToeState state, Player? player, int boardIndex, int? activeBoardIndex, int col, int row) {
        player = state.IsGameOver //show for all players if the game is over.
            ? null
            : player;

        var board = state.Boards[boardIndex];
        var foundSpace = board.Spaces[col,row].ToString(player);
        
        if (
            string.IsNullOrWhiteSpace(foundSpace) 
            && !board.IsDone 
            && state.PlayManager.CanTakeTurn(player)
            && activeBoardIndex == boardIndex
        ) {
            return board.GetSpaceNameAsInt(col, row)
                .ToString(new string('0', board.SpaceNameLength)); //zero-pad
        } else {
            return foundSpace;
        }
    }
}