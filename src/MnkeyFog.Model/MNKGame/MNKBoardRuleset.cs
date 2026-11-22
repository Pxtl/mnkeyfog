using System.ComponentModel;
using OneOf;
using OneOf.Types;

namespace MnkeyFog.Model.MNKGame;

/// <summary>
/// A board ruleset for an MNK game such as tic tac toe.  <see href="https://en.wikipedia.org/wiki/M,n,k-game">WP: MNK Game</see>
/// </summary>
[ModelSerializable]
[ImmutableObject(true)]
public record MNKBoardRuleset(sbyte? ScoringLength = null, bool IsBoardDoneWhenScored = false)
: BoardRuleset() {
    public static Template.BoardBuilder CreateBoardBuilder(
        sbyte columnCount,
        sbyte rowCount,
        sbyte? scoringLength = null,
        bool isBoardDoneWhenScored = false
    ) {
        if (columnCount > 26) {
            throw new ArgumentException("The board size limit is 26x26.", nameof(columnCount));
        }
        if (rowCount > 26) {
            throw new ArgumentException("The board size limit is 26x26.", nameof(rowCount));
        }

        return new Template.BoardBuilder(
            columnCount,
            rowCount,
            new MNKBoardRuleset(scoringLength, isBoardDoneWhenScored)
        ) { };
    }

    #region Methods
    public override ScoreCard Score(Board board) {
        var result = new ScoreCard();
        sbyte colCount = board.ColumnCount;
        sbyte rowCount = board.RowCount;

        var horizontalScoringLength = colCount;
        var verticalScoringLength = rowCount;
        var diagonalScoringLength = Math.Min(colCount, rowCount);

        if (ScoringLength.HasValue) {
            horizontalScoringLength = ScoringLength.Value;
            verticalScoringLength = ScoringLength.Value;
            diagonalScoringLength = ScoringLength.Value;
        }

        foreach (var spaceEnumerator in board.AsSpaceEnumerable()) {
            var lineOwnerMarkIndex = spaceEnumerator.Space.MarkIndex;
            // Impasse spaces (MarkIndex == -1) are not owned by any player, so
            // they are not scored.
            if (lineOwnerMarkIndex != null && lineOwnerMarkIndex.Value != Space.ImpasseMarkIndex) {
                result += ScoreSpace(
                    lineOwnerMarkIndex.Value,
                    board,
                    (spaceEnumerator.Col, spaceEnumerator.Row),
                    horizontalScoringLength,
                    verticalScoringLength,
                    diagonalScoringLength
                );
            }
        }
        return result;
    }

    public override bool IsDone(Board board)
    => IsBoardDoneWhenScored && board.ScoreCard.PlayerScores.Any(s => s.Score > 0);

    public override string ToString()
    => "m,n,k game scoring, "
        + (ScoringLength.HasValue
            ? $"{ScoringLength} in a row."
            : $"full width/height/diagonal of the board."
        );
    #endregion

    #region private helpers

    protected ScoreCard ScoreSpace(
        int lineOwnerPlayerIndex,
        Board board,
        (sbyte Col, sbyte Row) pos,
        sbyte horizontalScoringLength,
        sbyte verticalScoringLength,
        sbyte diagonalScoringLength
    )
    => ScoreSpace(lineOwnerPlayerIndex, board, pos, (1, 0), horizontalScoringLength)
        + ScoreSpace(lineOwnerPlayerIndex, board, pos, (0, 1), verticalScoringLength)
        + ScoreSpace(lineOwnerPlayerIndex, board, pos, (1, 1), diagonalScoringLength)
        + ScoreSpace(lineOwnerPlayerIndex, board, pos, (1, -1), diagonalScoringLength);

    /// <summary>
    /// Score a given space for the given player and the given direction. Only
    /// counts score for lines that *start* on the space, not ones that continue
    /// on the space.
    /// </summary>
    /// <param name="lineOwnerPlayerIndex"></param>
    /// <param name="lineStartPos"></param>
    /// <param name="delta"></param>
    /// <param name="scoreLen"></param>
    /// <returns></returns>
    protected ScoreCard ScoreSpace(
        int lineOwnerPlayerIndex,
        Board board,
        (sbyte Col, sbyte Row) lineStartPos,
        (sbyte Col, sbyte Row) delta,
        int scoreLen
    ) {
        (sbyte Col, sbyte Row) endPos = ExtrapolatePos(lineStartPos, delta, scoreLen - 1);

        //end point is outside of board.
        if (!board.IsSpaceInsideOfBoard(endPos)) {
            return ScoreCard.Empty;
        }

        (sbyte Col, sbyte Row) beforeStartPos = ExtrapolatePos(lineStartPos, delta, -1);
        if (
            board.IsSpaceInsideOfBoard(beforeStartPos)
            && board.Spaces[beforeStartPos.Col, beforeStartPos.Row].MarkIndex == lineOwnerPlayerIndex
        ) {
            // line already started before this space, return false to prevent double-counting.
            return ScoreCard.Empty;
        }

        var lineLength = 0;
        for (sbyte i = 0; board.IsSpaceInsideOfBoard(ExtrapolatePos(lineStartPos, delta, i)); i += 1) {
            (sbyte Col, sbyte Row) curPos = ExtrapolatePos(lineStartPos, delta, i);

            if (lineOwnerPlayerIndex != board.Spaces[curPos.Col, curPos.Row].MarkIndex) {
                break;
            } else {
                lineLength = i + 1;
            }
        }
        var lineScore = lineLength / scoreLen;
        return (lineScore > 0)
            ? new ScoreCard(lineOwnerPlayerIndex, lineScore)
            : ScoreCard.Empty;
    }

    private static (sbyte Col, sbyte Row) ExtrapolatePos((sbyte Col, sbyte Row) pos, (sbyte Col, sbyte Row) delta, int multiplier)
    => ((pos.Col + delta.Col * multiplier).AsSByte, (pos.Row + delta.Row * multiplier).AsSByte);
    #endregion

}
