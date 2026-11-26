namespace MnkeyFog.Model.Template;

/// <summary>
/// These objects define what types of moves can be done by the player(s) at any
/// given moment in the game. The game loop will request the list of
/// GameActionFactories, then the player will use one to construct an action,
/// and then it will be Attempted and possibly added to the action queue to
/// execute the move.
/// </summary>
public abstract record GameActionFactory {
    /// <summary>
    /// The <see cref="ModelName"/> of the action. The <c>CommandName</c> is what
    /// a player types after <c>!play</c> (e.g. <c>space</c>, <c>pass</c>); the
    /// <c>DisplayName</c> is how the action is shown in the UI.
    /// </summary>
    [JsonIgnore()]
    public abstract ModelName Name { get; }
}

public abstract record GameActionFactoryForSimple : GameActionFactory {
    public abstract GameAction Create();
}
public abstract record GameActionFactoryForSpace : GameActionFactory {
    public abstract GameAction Create(sbyte boardIndex, sbyte col, sbyte row);
}
public abstract record GameActionFactoryForColumn : GameActionFactory {
    public abstract GameAction Create(sbyte boardIndex, sbyte col);
}
public abstract record GameActionFactoryForRow : GameActionFactory {
    public abstract GameAction Create(sbyte boardIndex, sbyte row);
}
public abstract record GameActionFactoryForBoard : GameActionFactory {
    public abstract GameAction Create(sbyte boardIndex);
}
