namespace MnkeyFog.Model.Tests;

public class StateUtilityTests {
    [Fact]
    public void SerializeAndDeserialize_Board() {
        var expectedBoard = new Board(4, 4, new MNKBoardRuleset(3, true));
        var boardString = StateStorage.StateToString(expectedBoard);
        var actualBoard = StateStorage.StringToState<Board>(boardString);
        actualBoard.Should().BeEquivalentTo(expectedBoard);
    }

    [Fact]
    public void SerializeAndDeserialize_BlankGameState() {
        var boardBuilder3x3 = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        IGameState expectedState = new GameState(
            new char[] { 'X', 'O' }.ToPlayersArray(),
            new MNKTemplate([boardBuilder3x3, boardBuilder3x3, boardBuilder3x3], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        var stateString = StateStorage.StateToString(expectedState);
        var actualState = StateStorage.StringToState<IGameState>(stateString);
        actualState.Should().BeOfType(typeof(GameState));
        actualState.Should().BeEquivalentTo(expectedState);
    }

    [Fact]
    public void SerializeAndDeserialize_SynchronousGameState() {
        var boardBuilder3x3 = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        var players = new char[] { 'X', 'O' }.ToPlayersArray();
        var expectedState = new GameState(
            players,
            new MNKTemplate([boardBuilder3x3, boardBuilder3x3, boardBuilder3x3], isSynchronousMode: true, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        var playerXIndex = 0;
        var playerOIndex = 1;

        //round 1 (collision)
        expectedState.GetView(playerXIndex).Attempt(new MNKAction(0, 1, 1));
        expectedState.GetView(playerOIndex).Attempt(new MNKAction(0, 1, 1));
        expectedState.EndRound(out _);

        //round 2 (2 separate moves)
        expectedState.GetView(playerXIndex).Attempt(new MNKAction(0, 0, 0));
        expectedState.GetView(playerOIndex).Attempt(new MNKAction(0, 2, 2));
        expectedState.EndRound(out _);

        //round 3 (player O discovers player X)
        expectedState.GetView(playerXIndex).Attempt(new MNKAction(0, 2, 0));
        expectedState.GetView(playerOIndex).Attempt(new MNKAction(0, 0, 0));
        expectedState.EndRound(out _);

        //round 4 (incomplete)
        expectedState.GetView(playerXIndex).Attempt(new MNKAction(0, 1, 0));

        IGameState untypedExpectedState = expectedState;

        var stateString = StateStorage.StateToString<IGameState>(expectedState);
        var untypedActualState = StateStorage.StringToState<IGameState>(stateString);

        untypedActualState.Should().BeOfType(typeof(GameState));
        var actualState = (GameState)untypedActualState;

        untypedActualState.Boards.Should().BeEquivalentTo(untypedExpectedState.Boards);
        untypedActualState.PlayersState.Should().BeEquivalentTo(untypedExpectedState.PlayersState);
        actualState.ActionQueue.Should().BeEquivalentTo(expectedState.ActionQueue);
        actualState.Should().BeEquivalentTo(expectedState);
    }
}
