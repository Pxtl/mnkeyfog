namespace MnkeyFog.Model.Tests;

public class PassActionTests {
    private static GameState NewState()
    => new(
        (new[] { 'X', 'O' }).ToPlayersArray(),
        new MNKTemplate(
            [new BoardBuilder(3, 3, new MNKBoardRuleset(ScoringLength: null, IsBoardDoneWhenScored: true))],
            isSynchronousMode: false,
            isKriegspiel: false
        ),
        isRandomPlayerOrder: false
    );

    [Fact]
    public void PassActionFactory_Name_IsPass() {
        new PassActionFactory().Name.Should().Be(new ModelName("pass", "pass"));
    }

    [Fact]
    public void MNKActionFactory_Name_IsSpace() {
        new MNKActionFactory().Name.Should().Be(new ModelName("space", "space"));
    }

    [Fact]
    public void PassActionFactory_Create_ReturnsPassAction() {
        new PassActionFactory().Create().Should().Be(new PassAction());
    }

    [Fact]
    public void PassAction_Attempt_AdvancesTurnAndDoesNotChangeBoard() {
        var state = NewState();
        var playerX = state.PlayersState.GetPlayer("X");
        var playerO = state.PlayersState.GetPlayer("O");
        state.PlayersState.CanTakeTurn(playerX.Index).Should().BeTrue();

        var result = state.GetView(playerX).Attempt(new PassAction());
        state.ExecutePendingActions();

        result.IsTurnDone.Should().BeTrue();
        state.PlayersState.CanTakeTurn(playerX.Index).Should().BeFalse();
        state.PlayersState.CanTakeTurn(playerO.Index).Should().BeTrue();
        state.Boards[0].AsSpaceEnumerable().Should().OnlyContain(s => s.Space.MarkIndex == null);
    }

    [Fact]
    public void PassAction_Attempt_OutOfTurn_ReturnsCannotTakeTurn() {
        var state = NewState();
        var playerX = state.PlayersState.GetPlayer("X");
        state.GetView(playerX).Attempt(new PassAction()); // X passes; turn -> O

        var result = state.GetView(playerX).Attempt(new PassAction());

        result.IsTurnDone.Should().BeFalse();
        result.IsViewChanged.Should().BeFalse();
    }

    [Fact]
    public void PassAction_IsActionCollision_ReturnsFalse() {
        var state = NewState();
        var playerX = state.PlayersState.GetPlayer("X");
        var pass = new PassAction();
        pass.IsActionCollision(pass.GetPlayerAction(playerX.Index), playerX.Index).Should().BeFalse();
    }
}
