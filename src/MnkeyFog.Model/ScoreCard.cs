namespace MnkeyFog.Model;

using Sundew.Base.Collections.Immutable;
using OneOf.Types;

/// <summary>
/// Immutable value-y collection of scores
/// </summary>
[ModelSerializable]
public struct ScoreCard {
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
    private static ValueArray<PlayerScore> _emptyPlayerScoreCollection = new ValueArray<PlayerScore>();
    public static ScoreCard Empty { get; } = new ScoreCard();

    #endregion

    #region state members
    private ValueArray<PlayerScore> _scores {get; set;}
    public readonly IReadOnlyList<PlayerScore> PlayerScores
        => _scores;
    #endregion

    #region calculated members
    public readonly bool IsEmpty
    => _scores.Count == 0;

    public readonly ScoreCard Highest
    => _scores.Count == 0 
        ? Empty
        : new ScoreCard(_scores.AllMaxBy(s => s.Score));

    public readonly IEnumerable<Player> Players => _scores.Select(s => s.Player);
	#endregion

	#region object overrides (equality and tostring)
	public override string? ToString() => _scores.ToString();

    public override bool Equals(object? obj) {
        if (obj == null) {
            return false;
        } else if (obj is PlayerScore playerScore) {
            obj = new ScoreCard(playerScore);
        } 
        
        if (obj is ScoreCard scoreCard) {
            return _scores.Equals(scoreCard._scores);
        } else {
            return false;
        }
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    => _scores.GetHashCode();

    public static bool operator== (ScoreCard? a, ScoreCard? b) {
        if(a == null) {
            return b == null;
        } else {
            return a.Equals(b);
        }
    }

    public static bool operator!= (ScoreCard? a, ScoreCard? b) {
        return !(a == b);
    }
    #endregion

	public ScoreCard FilterByPlayers(IEnumerable<Player> players)
	=> new ScoreCard(PlayerScores.Where(ps => players.Contains(ps.Player)));

    #region operator overloads
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

/// <summary>
/// Allow the above static <see cref="ScoreCard.SumScoreCards(IEnumerable{ScoreCard})"/> to be used as extension.
/// </summary>
public static class ScoreCardExtensions {
    /// <summary>
    /// can't use ienumerable.sum on non-numeric objects, operators don't work
    /// that way, so we have to create a sum method.
    /// </summary>
    public static ScoreCard SumScoreCards(this IEnumerable<ScoreCard> scoreCards)
    => ScoreCard.SumScoreCards(scoreCards);

    public static ScoreCard BlankPlayersScoreCard(this IEnumerable<Player> players)
    => players
        .Select(p => new ScoreCard(new PlayerScore(p, 0)))
        .SumScoreCards();
}
