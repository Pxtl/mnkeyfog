namespace KriegspielTicTacToe.Model;

using Sundew.Base.Collections.Immutable;
using OneOf.Types;

/// <summary>
/// Immutable value-y collection of scores
/// </summary>
public record struct ScoreCard {
    #region constructors
    public ScoreCard() {
        _scores = _emptyPlayerScoreCollection;
    }
    public ScoreCard(Player player, int score) : this(new PlayerScore(player, score)) {}
    public ScoreCard(PlayerScore playerScore) : this([playerScore]) {}
    public ScoreCard(IEnumerable<PlayerScore> scores) {
        _scores = scores
            .GroupBy(s => s.Player)
            .OrderBy(g => g.Key.Mark)
            .Select(g => new PlayerScore(g.Key, g.Sum(kvp => kvp.Score)))
            .ToValueArray();
    }

    #endregion

    #region static Empty
    private static ScoreCard _empty = new ScoreCard();
    private static ValueArray<PlayerScore> _emptyPlayerScoreCollection = new ValueArray<PlayerScore>();
    public static ScoreCard Empty
        => _empty;

    #endregion

    #region state members
    public ValueArray<PlayerScore> _scores {get;private set;}
    public readonly IReadOnlyList<PlayerScore> PlayerScores
        => _scores;
    #endregion

    #region calculated members
    public bool IsEmpty
        => _scores.Count == 0;

    public PlayerScore? HighestScore
        => _scores.Count == 0 
            ? null
            : _scores.MaxByStrict(s => s.Score);
            
    #endregion

    #region overloads
    public static ScoreCard operator +(ScoreCard a, ScoreCard b)
        => a.IsEmpty ? b // optimization, if a or b are empty just use the other one directly.
        : b.IsEmpty ? a
        : new ScoreCard(a._scores.Concat(b._scores));

    public static ScoreCard operator +(ScoreCard a, PlayerScore b)
        => new ScoreCard(a._scores.Append(b));

    public static ScoreCard operator +(PlayerScore a, ScoreCard b)
        => new ScoreCard(a) + b;
    #endregion

    public static ScoreCard SumScoreCards(IEnumerable<ScoreCard> scoreCards)
        => new ScoreCard(scoreCards.SelectMany(s => s._scores));
}

public static class ScoreCardExtensions {
    public static ScoreCard SumScoreCards(this IEnumerable<ScoreCard> scoreCards)
        => ScoreCard.SumScoreCards(scoreCards);
}
