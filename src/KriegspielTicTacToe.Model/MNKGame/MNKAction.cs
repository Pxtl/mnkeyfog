using System.ComponentModel.DataAnnotations;
namespace KriegspielTicTacToe.Model.MNKGame;

/// <summary>
/// A play action for an MNK game such as tic tac toe.  <see href="https://en.wikipedia.org/wiki/M,n,k-game">WP: MNK Game</see>
/// </summary>
[ModelSerializable]
public record MNKAction
: GameAction {
    [Obsolete("Default constructor is only used for deserialization.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public MNKAction() : base() { }
#pragma warning restore CS8618
	public MNKAction(
        sbyte boardIndex,
        sbyte col,
        sbyte row
    ) : base() {
        BoardIndex = boardIndex;
        Col = col;
        Row = row;
    }
    [Required]
    public sbyte BoardIndex {get;init;}
    [Required]
    public sbyte Col {get;init;}
    [Required]
    public sbyte Row {get;init;}
 
	public override void DoActionCollision(GameState gameState, Player actionPlayer, IReadOnlyList<PlayerAction> collisions) {
        if (GetBoard(gameState).IsDone) {
            return;
        }
        var space = GetSpace(gameState);
		space.Mark = "█";
        foreach(var player in collisions.Select(c => c.Player)) {
            space.MakeKnownToPlayer(player);
        }
	}

    protected Board GetBoard(GameState gameState)
        => gameState.Boards[BoardIndex];

    protected Space GetSpace(GameState gameState)
        => GetBoard(gameState).Spaces[Col, Row];

	public override bool IsActionCollision(PlayerAction otherAction, Player actionPlayer)
    => otherAction.GameAction is MNKAction otherTicTacToeAction 
        ? BoardIndex == otherTicTacToeAction.BoardIndex
            && Col == otherTicTacToeAction.Col
            && Row == otherTicTacToeAction.Row
            && actionPlayer != otherAction.Player
        : throw new InvalidOperationException("Cannot compare different action types.");

	public override IPlayActionResult Attempt(GameState gameState, Player actionPlayer) {
        if(!gameState.PlayManager.CanTakeTurn(actionPlayer)) {
            return new InvalidCommand(actionPlayer.Mark);
        }
        if(BoardIndex < 0 || BoardIndex >= gameState.Boards.Count) {
            return new InvalidCommand(BoardIndex.ToString());
        }
        var board = gameState.Boards[BoardIndex];
        if(!board.IsSpaceInsideOfBoard((Col, Row), (board.ColumnCount, board.RowCount))) {
            return new InvalidCommand($"{Col}, {Row}");
        }

        var space = board.Spaces[Col, Row];
        if (space.Mark == null) {
            space.MakeKnownToPlayer(actionPlayer);
            gameState.ActionQueue.Add(GetPlayerAction(actionPlayer));
            gameState.PlayManager.EndTurn(actionPlayer, out var hasStateChanged);
            var spaceName = gameState.GetView(actionPlayer).GetSpaceName(BoardIndex, Col, Row);
            return new Enqueued(hasStateChanged, spaceName);
        } else if (space.IsKnownToPlayer(actionPlayer)) {
            return new AlreadyPlayed(actionPlayer);
        } else {
            space.MakeKnownToPlayer(actionPlayer);
            gameState.PlayManager.EndTurn(actionPlayer, out _);
            return new NewlyLearned(space.Mark);
        }
	}

	public override void DoAction(GameState gameState, Player actionPlayer)
	{
		if (GetBoard(gameState).IsDone) {
            return;
        }
        var space = GetSpace(gameState);

        if (space.Mark == null) {
            space.Mark = actionPlayer.Mark;
        }
            
        space.MakeKnownToPlayer(actionPlayer);
	}

    public static GameAction Create(
        GameState gameState,
        string spaceName
    ) {
        if (gameState.GetView(player: null).TryGetCoordinatesFromSpaceName(spaceName, out sbyte boardIndex, out var col, out var row)) {
            return new MNKAction(boardIndex, col, row);
        } else {
            throw new KeyNotFoundException("That is not a valid space name.");
        }
    }
}
