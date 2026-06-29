using System.Runtime.Serialization;
using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.Views;

namespace KriegspielTicTacToe.Model;

[ModelSerializable]
public record GameState
: IGameState, IGameStateServer {
    #region Constructors
    public GameState() { 
        // unusable default values will probably get removed when members are
        // initialized.
        PlayManager = new RoundRobinPlayManager([]);
        Boards = [];
        GameTemplate = null!;
    }

    public GameState(
        Player[] players,
        IGameTemplate gameTemplate,
        bool isRandomPlayerOrder
    ) {
        if(isRandomPlayerOrder) {
            Random.Shared.Shuffle(players);
        }

        GameTemplate = gameTemplate;
        PlayManager = gameTemplate.PlayManagerFactory.Create(players);        
        Boards = gameTemplate.CreateBoards();
        Initialize();

        if (Boards.Count > 1 && Boards.Any(b => b.RowCount > 9)) {
            throw new ApplicationException(
                "Cannot start game. Current board-renderer does not support boards that are taller than 9 spaces in multi-board games."
            );
        }

        if (!gameTemplate.LegalPlayerCounts.Contains(players.Length)) {
            throw new ApplicationException(
                "Cannot start game. This game only supports the following player-counts: "
                    + string.Join(", ", gameTemplate.LegalPlayerCounts)
                    + Environment.NewLine
                    + $"You have provided {players.Length} player(s)."
            );
        }

        gameTemplate.InitializeGame(this);
    }
    #endregion

    #region Data Properties
    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    public IGameTemplate GameTemplate { get; init; }

    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    public virtual PlayManager PlayManager { get; init; }

    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.None)]
    public virtual IReadOnlyList<Board> Boards { get; init; }

    [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
    public PlayActionQueue ActionQueue { get; init; } = new();
    #endregion

    #region Initializer

    #region IGameStateServer
    public IPlayActionResult Attempt(PlayerAction action) => action.Attempt(this);
    public void ResignPlayer(Player player) => PlayManager.ResignPlayer(player);
    #endregion
    public virtual void Initialize() {
        ActionQueue.GameState = this;
        PlayManager.ActionQueue = ActionQueue;
    }

    [OnDeserialized]
    public void Initialize(StreamingContext context) {
        Initialize();
    }
    #endregion

    public GameView GetView(Player? player)
    => new(this, player);

    #region Players and Scores
    /// <summary>
    /// Scorecard for all active (non-resigned) players
    /// </summary>
    [JsonIgnore()]
    public ScoreCard ScoreCard 
    => AllPlayersScoreCard.FilterByPlayers(PlayManager.ActivePlayers);

    
    [JsonIgnore()]
    public ScoreCard AllPlayersScoreCard
    => PlayManager.Players.BlankPlayersScoreCard()  //make sure all active players are in the scorecard even those with 0.
        + Boards.Select(b => b.ScoreCard).SumScoreCards();


    [JsonIgnore()]
    public virtual IEnumerable<Player> Winners { get {
        if(!IsGameOver) {
            return [];
        }
        if(PlayManager.ActivePlayers.Count() == 1) {
            return PlayManager.ActivePlayers;
        }
        else {
            return ScoreCard.Highest.Players;
        }
    }}

    [JsonIgnore()]
    public bool IsGameOver
    => Boards.All(b => b.IsDone) || PlayManager.ActivePlayers.Count() == 1;


    [JsonIgnore()]
    public string GameStateText
    => (
            IsGameOver 
            ? (Winners.Count() == 0
                ? "Game over. Nobody wins."
                : $"Game over. {string.Join(" and ", Winners)} win(s)."
            ) 
            : PlayManager.GameStateText
        )
            + Environment.NewLine 
            + ResignedPlayersText;

    [JsonIgnore()]
    public string ResignedPlayersText
    => PlayManager.ResignedPlayersSet.Count > 0
        ? $"Resigned players: {string.Join(", ", PlayManager.ResignedPlayersSet.OrderBy(p => p.Mark))}"
        : "";
    #endregion

    #region board management

    [JsonIgnore()]
    public IEnumerable<sbyte> ActiveBoardIndices { get {
        for(sbyte i = 0; i < Boards.Count; i+=1) {
            if(!Boards[i].IsDone) {
                yield return i; 
            }
        }
    }}
    
    /// <summary>
    /// If there is only one active board, return its index.  Otherwise, return
    /// null.
    /// </summary>
    [JsonIgnore()]
    public sbyte? SingleActiveBoardIndex { get {
        var firstElements = ActiveBoardIndices.Take(2).ToArray();
        return (firstElements.Length == 1) ? firstElements.Single() : null;
    }}

    public Board GetBoardByIndex(sbyte boardIndex)
    => Boards[boardIndex];

	#endregion
}
