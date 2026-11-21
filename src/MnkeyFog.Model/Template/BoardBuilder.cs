namespace MnkeyFog.Model.Template;

/// <summary>
/// Parameters to create a board, including the scoring settings for the board.  Currently only used by the <see cref="TicTacToeTemplate"/>.
/// </summary>
[ModelSerializable]
public record BoardBuilder(sbyte ColumnCount, sbyte RowCount, BoardRuleset Ruleset = null!) {
    public BoardRuleset Ruleset = Ruleset ?? BoardRuleset.Empty;
    public override string ToString()
    => $"{ColumnCount}x{RowCount}, ruleset {Ruleset}";
};
