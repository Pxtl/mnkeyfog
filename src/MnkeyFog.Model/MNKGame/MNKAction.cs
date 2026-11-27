using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace MnkeyFog.Model.MNKGame;

/// <summary>
/// A play action for an MNK game such as tic tac toe.  <see href="https://en.wikipedia.org/wiki/M,n,k-game">WP: MNK Game</see>
/// </summary>
[ModelSerializable]
[ImmutableObject(true)]
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
    public sbyte BoardIndex { get; init; }
    [Required]
    public sbyte Col { get; init; }
    [Required]
    public sbyte Row { get; init; }

    public override void DoActionCollision(GameState gameState, int actionPlayerIndex, IReadOnlyList<PlayerAction> collisions) {
        if (GetBoard(gameState).IsDone) {
            return;
        }
        var space = GetSpace(gameState);
        space.MarkIndex = Space.ImpasseMarkIndex;
        foreach (var playerIndex in collisions.Select(c => c.PlayerIndex)) {
            var player = gameState.PlayersState.GetPlayer(playerIndex);
            space.MakeKnownToPlayerIndex(player.Index);
        }
    }

    protected Board GetBoard(GameState gameState)
        => gameState.Boards[BoardIndex];

    protected Space GetSpace(GameState gameState)
        => GetBoard(gameState).Spaces[Col, Row];

    public override bool IsActionCollision(PlayerAction otherAction, int actionPlayerIndex)
    => otherAction.GameAction switch {
        MNKAction otherMNKAction
            => BoardIndex == otherMNKAction.BoardIndex
                && Col == otherMNKAction.Col
                && Row == otherMNKAction.Row
                && actionPlayerIndex != otherAction.PlayerIndex,
        // a pass touches no space, so it can never collide with a space play.
        PassAction => false,
        _ => throw new InvalidOperationException("Cannot compare these action types.")
    };

    public override IPlayActionResult Attempt(GameState gameState, int actionPlayerIndex) {
        if (!gameState.PlayersState.CanTakeTurn(actionPlayerIndex)) {
            return new CannotTakeTurn(actionPlayerIndex);
        }
        if (BoardIndex < 0 || BoardIndex >= gameState.Boards.Count) {
            return new InvalidCommand(BoardIndex.ToString());
        }
        var board = gameState.Boards[BoardIndex];
        if (!board.IsSpaceInsideOfBoard((Col, Row))) {
            return new InvalidCommand($"{Col}, {Row}");
        }

        ref var space = ref board.Spaces[Col, Row];
        if (space.MarkIndex == null) {
            space.MakeKnownToPlayerIndex(actionPlayerIndex);
            gameState.ActionQueue.Add(GetPlayerAction(actionPlayerIndex));
            gameState.EndTurn(actionPlayerIndex, out var hasStateChanged);
            return new Enqueued(hasStateChanged);
        } else if (space.IsKnownToPlayerIndex(actionPlayerIndex)) {
            return new PositionAlreadyPlayed(actionPlayerIndex);
        } else {
            space.MakeKnownToPlayerIndex(actionPlayerIndex);
            gameState.EndTurn(actionPlayerIndex, out _);
            return new NewlyLearned(space.MarkIndex.Value);
        }
    }

    public override void DoAction(GameState gameState, int actionPlayerIndex) {
        if (GetBoard(gameState).IsDone) {
            return;
        }
        var space = GetSpace(gameState);

        if (space.MarkIndex == null) {
            space.MarkIndex = actionPlayerIndex;
        }
        space.MakeKnownToPlayerIndex(actionPlayerIndex);
    }
}
