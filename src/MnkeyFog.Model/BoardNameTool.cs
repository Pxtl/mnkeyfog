namespace MnkeyFog.Model;

using OneOf;
using OneOf.Types;

/// <summary>
/// Provides static methods for mapping board names and board indices.
/// </summary>
/// <remarks>
/// <para>
/// Board names are the 1-based board number rendered as a string.
/// </para>
/// <para>
/// These methods are used by both the Model and Text layers for player
/// commands.
/// </para>
/// </remarks>
public static class BoardNameTool {
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

    public static string BoardNameFromIndex(int boardIndex)
    => (boardIndex + 1).ToString();

    public static OneOf<NotFound, Result<sbyte>> GetBoardIndexByName(string boardName, int boardsCount) {
        if (TryGetBoardIndexByName(boardName, boardsCount, out var boardIndex)) {
            return new Result<sbyte>(boardIndex);
        } else {
            return new NotFound();
        }
    }
}
