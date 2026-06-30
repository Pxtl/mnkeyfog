namespace MnkeyFog.Model;

/// <summary>
/// Parameter and logic for scoring a board. Default implementation gives nobody
/// any points ever, you must override Score to get actual useful gameplay.
/// </summary>
[ModelSerializable]
public record BoardRuleset {
	public static BoardRuleset Empty { get; } = new BoardRuleset();
	public virtual bool IsDone(Board board)
	=> false;

	public virtual ScoreCard Score(Board board)
	=> ScoreCard.Empty;

	public override string ToString()
	=> "empty rules (scoring is impossible)";
};
