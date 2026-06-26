namespace KriegspielTicTacToe.Model.Template;

/// <summary>
/// Parameters to create a board, including the scoring settings for the board.  Currently only used by the <see cref="TicTacToeTemplate"/>.
/// </summary>
[ModelSerializable]
public record BoardBuilder(sbyte Width, sbyte Height, BoardRuleset Ruleset = null!) {
    public BoardRuleset Ruleset = Ruleset ?? BoardRuleset.Empty;
	public string ToString(string boardName) 
    => $"Board {boardName}:" + ToString();
	public override string ToString()
	=> $"{Width}x{Height}, ruleset {Ruleset}";
};
