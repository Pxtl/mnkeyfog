using System.ComponentModel;
using MnkeyFog.Model.Template;

namespace MnkeyFog.Model.MNKGame;

/// <summary>
/// Represents a game type configuration including board builders and play mode settings for an MNK game such as tic tac toe.  <see href="https://en.wikipedia.org/wiki/M,n,k-game">WP: MNK Game</see>
/// </summary>
[ModelSerializable]
[ImmutableObject(true)]
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
        IReadOnlyList<BoardBuilder> boardBuilders,
        bool isKriegspiel,
        bool isSynchronousMode
    ) : this(boardBuilders, isKriegspiel, isSynchronousMode) {
        CommandName = commandName;
        Description = description;
        LegalPlayerCounts = legalPlayerCounts;
    }

    public MNKTemplate(
        IReadOnlyList<BoardBuilder> boardBuilders,
        bool isKriegspiel,
        bool isSynchronousMode
    )
    : base() {
        BoardBuilders = boardBuilders;
        IsKriegspiel = isKriegspiel;
        PlayManager = isSynchronousMode
            ? SynchronizedPlayManager.Instance
            : RoundRobinPlayManager.Instance;
    }
    #endregion

    #region data members
    /// <summary>
    /// The default action for an MNK game is a space move.
    /// </summary>
    public override string DefaultActionCommandName => "space";

    public bool IsKriegspiel { get; init; }

    [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
    public IReadOnlyList<BoardBuilder> BoardBuilders { get; init; }
    #endregion

    public override IReadOnlyList<Board> CreateBoards()
    => BoardBuilders.Select(b => new Board(b)).ToList();

    public override void InitializeGame(GameState gameState) {
        if (!IsKriegspiel) {
            foreach (var board in gameState.Boards) {
                foreach (var spaceEnum in board.AsSpaceEnumerable()) {
                    for (var i = 0; i < gameState.PlayersState.PlayerInfos.Count; i += 1) {
                        spaceEnum.Space.MakeKnownToPlayerIndex(i);
                    }
                }
            }
        }
    }

    public override IEnumerable<GameActionFactory> GetAvailableActions(GameState gameState, int playerIndex)
    => gameState.PlayersState.CanTakeTurn(playerIndex)
        ? [new MNKActionFactory()]
        : [];
}
