using KriegspielTicTacToe.Model.Template;

namespace KriegspielTicTacToe.Model.MNKGame;

/// <summary>
/// Represents a game type configuration including board builders and play mode settings for an MNK game such as tic tac toe.  <see href="https://en.wikipedia.org/wiki/M,n,k-game">WP: MNK Game</see>
/// </summary>
public record MNKTemplate
: GameTemplate {
    #region constructors
    public MNKTemplate() : base() {
        BoardBuilders = [];
    }

    public MNKTemplate(
        string commandName,
        string description,
        IEnumerable<int> legalPlayerCounts,
        IEnumerable<BoardBuilder> boardBuilders,
        bool isKriegspiel,
        bool isSynchronousMode
    ) : this(boardBuilders, isKriegspiel, isSynchronousMode) {
        CommandName = commandName;
        Description = description;
        LegalPlayerCounts = legalPlayerCounts;
    }

    public MNKTemplate(
        IEnumerable<BoardBuilder> boardBuilders,
        bool isKriegspiel,
        bool isSynchronousMode
    )
    : base() {
        BoardBuilders = boardBuilders;
        IsKriegspiel = isKriegspiel;
        PlayManagerFactory = isSynchronousMode 
            ? SynchronizedPlayManagerFactory.Instance
            : RoundRobinPlayManagerFactory.Instance;
    }
    #endregion

    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    public IEnumerable<BoardBuilder> BoardBuilders {get; init;}

    public bool IsKriegspiel { get; init; }

    public override IReadOnlyList<Board> ConstructBoards()
    => BoardBuilders.Select(b => new Board(b)).ToList();

    public override void InitializeGame(IGameState gameState) {
        if (!IsKriegspiel) {
            foreach (var board in gameState.Boards) {
                foreach (var space in board.BoardAsSpaceEnumerable()) {
                    foreach(var player in gameState.PlayManager.Players) {
                        space.MakeKnownToPlayer(player.Mark);
                    }
                }
            }
        }
    }
}
