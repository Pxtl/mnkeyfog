namespace MnkeyFog.Model;

using OneOf;
using OneOf.Types;

/// <summary>
/// PlayManager for synchronized mode - player moves are buffered until round end.
/// </summary>
[ModelSerializable]
public class SynchronizedPlayManager 
: PlayManager {
    #region constructors
    public SynchronizedPlayManager(IReadOnlyList<Player> players) : base(players) { }
    #endregion

    #region properties
    [JsonIgnore()]
    public override string GameStateText
        => "Synchronized play. "
        + (PlayersAvailableForTurn.Any()
            ? $"Player(s) { string.Join(", ", PlayersAvailableForTurn)} have not taken their turn."
            : "Round complete."
        );

    protected override void EndedRound(out bool hasStateChanged) {
        ActionQueue!.ExecutePendingActions();
        hasStateChanged = true;
    }

    protected override void EndedTurn(out bool hasStateChanged) {
        hasStateChanged = false;
    }
    
    [JsonIgnore()]
    public override IEnumerable<Player> PlayersAvailableForTurn
        => ActivePlayers.Except(PlayedPlayersSet);
    #endregion
}

[ModelSerializable]
public record SynchronizedPlayManagerFactory
: Template.PlayManagerFactory {
    public static SynchronizedPlayManagerFactory Instance {get;} = new SynchronizedPlayManagerFactory();
	public override PlayManager Create(IReadOnlyList<Player> players) 
		=> new SynchronizedPlayManager(players);
}