namespace MnkeyFog.Model.Views;

/// <summary>
/// Abstract base class for a single player's views of Game Objects (or no
/// player for omniscient spectator).
/// </summary>
public abstract record GameObjectView {
    /// <summary>
    /// Construct a new view.  
    /// </summary>
    /// <param name="GameState">The gamestate to wrap.</param>
    /// <param name="Player">The player's view of that gamestate.  Null for omniscient spectator.</param>
    public GameObjectView(int? playerIndex) {
        PlayerIndex = playerIndex;
    }

    #region Data Properties
    [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
    public int? PlayerIndex { get; init; }
    #endregion
}
