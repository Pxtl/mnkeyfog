namespace KriegspielTicTacToe.Model;

using OneOf.Types;

/// <summary>
/// Read-only value-y collection of scores
/// </summary>
public record ScoreCard {
    #region constructors
    public ScoreCard() {
        Scores = Array.Empty<PlayerScore>();
    }
    public ScoreCard(Player player, int score) : this(new PlayerScore(player, score)) {}
    public ScoreCard(PlayerScore playerScore) : this(new[]{playerScore}) {}
    public ScoreCard(IEnumerable<PlayerScore> scores) {
        Scores = scores
            .GroupBy(s => s.Player)
            .Select(g => new PlayerScore(g.Key, g.Sum(kvp => kvp.Score)))
            .ToArray();
    }

    #endregion

    private PlayerScore[] Scores {get;set;}

    public PlayerScore? HighestScore
        => Scores.Length == 0 
            ? null
            : Scores.MaxByStrict(s => s.Score);

    public static ScoreCard operator +(ScoreCard a, ScoreCard b)
        => new ScoreCard(a.Scores.Concat(b.Scores));
    public static ScoreCard operator +(ScoreCard a, PlayerScore b)
        => new ScoreCard(a.Scores.Append(b));
    public static ScoreCard operator +(PlayerScore a, ScoreCard b)
        => new ScoreCard(a) + b;
}
