using MnkeyFog.Model.Template;
using OneOf;
using OneOf.Types;

namespace MnkeyFog.Model.Views;

/// <summary>
/// Player-based wrapper for GameState that restricts the API only to things
/// that a player can do and see.
/// </summary>
public record GameView
: GameObjectView {
    #region Constructors
    public GameView(GameState gameState, int? playerIndex)
    : base(playerIndex) {
        GameTemplate = gameState.GameTemplate;
        GameStateServer = gameState;
        IsGameOver = gameState.IsGameOver;
        AvailableActions = (PlayerIndex == null)
            ? new List<GameActionFactory>()
            : gameState.GameTemplate.GetAvailableActions(gameState, PlayerIndex.Value).ToList();

        var boardViewsArray = new BoardView[gameState.Boards.Count];
        for (sbyte i = 0; i < gameState.Boards.Count; i += 1) {
            boardViewsArray[i] = new BoardView(gameState.Boards[i], PlayerIndex, i, gameState.IsGameOver);
        }
        Boards = boardViewsArray;
        PlayersState = gameState.PlayersState;
    }
    #endregion

    #region Data Members
    public IGameTemplate GameTemplate { get; init; }
    public IGameStateServer GameStateServer { get; init; }
    public IReadOnlyList<BoardView> Boards { get; init; }
    public PlayersState PlayersState { get; init; }
    #endregion

    #region Copied Calculated Members
    public bool IsGameOver { get; set; }
    #endregion

    #region player actions
    public bool CanTakeTurn => PlayersState.CanTakeTurn(PlayerIndex);
    public IReadOnlyList<GameActionFactory> AvailableActions { get; init; }
    public Resigned ResignPlayer() {
        if (PlayerIndex == null) {
            throw new InvalidOperationException($"{nameof(PlayerIndex)} is null.");
        } else {
            GameStateServer.ResignPlayer(PlayerIndex.Value);
            return new Resigned(PlayerIndex.Value);
        }
    }

    public IPlayActionResult Attempt(GameAction playAction) {
        if (PlayerIndex == null) {
            throw new InvalidOperationException($"{nameof(PlayerIndex)} is null.");
        }
        return GameStateServer.Attempt(playAction.GetPlayerAction(PlayerIndex.Value));
    }

    //TODO: Row and Column attempt functions.
    #endregion

    #region helpers
    public PlayerInfo? PlayerInfo => PlayerIndex.HasValue
        ? PlayersState.PlayerInfos[PlayerIndex.Value]
        : null;
    #endregion

    #region board management
    [JsonIgnore()]
    public sbyte BoardsCount => Boards.Count.AsSByte;

    public BoardView GetBoardViewByIndex(sbyte boardIndex)
    => Boards[boardIndex];
    #endregion

    #region scores

    /// <summary>
    /// Possibly-approximate scorecard (depending on game) for all players
    /// </summary>
    [JsonIgnore()]
    public ScoreCard ScoreCard
    => PlayersState.PlayerInfos.BlankPlayersScoreCard()  //make sure all active players are in the scorecard even those with 0.
        + Boards.Select(b => b.ScoreCard).SumScoreCards();
    #endregion

    [JsonIgnore()]
    public IEnumerable<SpaceView> SpaceViews
    => Boards.SelectMany(b => b.AsSpaceViewEnumerable());
}

