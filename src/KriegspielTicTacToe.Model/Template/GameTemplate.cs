namespace KriegspielTicTacToe.Model.Template;

/// <summary>
/// A game template includes all the metadata needed to describe the type of a
/// game, but not the actual list of players or active state.
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
	/// Explicit list of how many players are allowed for the game.
	/// </summary>
	public IEnumerable<int> LegalPlayerCounts { get; init; } = DefaultLegalPlayerCounts;
	/// <summary>
	/// Used to control whether the game will run in round-robin or synchronous mode.
	/// </summary>
    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
	public PlayManagerFactory PlayManagerFactory { get; init; } = RoundRobinPlayManagerFactory.Instance;
	public abstract IEnumerable<GameActionFactory> GetAvailableActions(GameState gameState, Player player);
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
	public static IReadOnlyList<int> DefaultLegalPlayerCounts {get;} = Enumerable.Range(2, 32).ToList();
}