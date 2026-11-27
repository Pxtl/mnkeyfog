namespace MnkeyFog.Model.Template;

/// <summary>
/// A game template includes all the metadata needed to describe the type of a
/// game, but not the actual list of players or active state.  GameTemplates
/// must be immutable because the copy constructor of the Board assumes they can
/// be aliased safely.
/// </summary>
[ModelSerializable]
public abstract record GameTemplate()
: IGameTemplate {
    /// <summary>
    /// Name printed on the screen at the start of the game and (for built-in
    /// games) used to select the game from the command-line.
    /// </summary>
    public string? CommandName { get; init; }
    /// <summary>
    /// Description printed on the screen at the start of the game and (for
    /// built-in games) in the command-line help.
    /// </summary>
    public string? Description { get; init; }
    /// <summary>
    /// The <see cref="ModelName.CommandName"/> of this template's default action —
    /// the action shown as the default in multi-action pickers. Must match the
    /// command name of one of this template's action factories.
    /// </summary>
    public abstract string DefaultActionCommandName { get; }
    /// <summary>
    /// Explicit list of how many players are allowed for the game.
    /// </summary>
    public IEnumerable<int> LegalPlayerCounts { get; init; } = DefaultLegalPlayerCounts;
    /// <summary>
    /// Used to control whether the game will run in round-robin or synchronous mode.
    /// </summary>
    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    public PlayManager PlayManager { get; init; } = RoundRobinPlayManager.Instance;
    public abstract IEnumerable<GameActionFactory> GetAvailableActions(GameState gameState, int playerIndex);
    /// <summary>
    /// Create the boards for the game. See GameState for the constraints on the
    /// sizes and numbers of boards.
    /// </summary>
    public abstract IReadOnlyList<Board> CreateBoards();
    /// <summary>
    /// Executed at the start of the game after all basic construction is
    /// complete, allowing the template to alter the state of the game before
    /// the players touch it.
    /// </summary>
    public abstract void InitializeGame(GameState gameState);
    public static IReadOnlyList<int> DefaultLegalPlayerCounts { get; } = Enumerable.Range(2, 32).ToList();
}
