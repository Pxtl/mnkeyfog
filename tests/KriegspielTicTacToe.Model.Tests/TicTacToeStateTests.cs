namespace KriegspielTicTacToe.Model.Tests;

public class TicTacToeStateTests {
    [Fact]
    public void Constructor_EmptyBoards() {
        var state = new TicTacToeState(
            new[] { 'X', 'O' },
            Array.Empty<BoardBuilder>(),
            isRandomPlayerOrder: false,
            isSynchronousMode: false
        );
        state.Boards.Should().BeEmpty();
        state.PlayManager.Players.Should().Contain(new Player("X")).And.Subject.Should().Contain(new Player("O"));
        state.PlayManager.ActivePlayers.Should().Contain(new Player("X")).And.Subject.Should().Contain(new Player("O"));
    }

    [Fact]
    public void Constructor_WithBoardsCreatesProperState() {
        var state = new TicTacToeState(
            [new Player("X"), new Player("O")],
            [new BoardBuilder(3, 3), new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: false
        );
        state.Boards.Count.Should().Be(2);
        state.PlayManager.Players.Should().Contain(new Player("X")).And.Subject.Should().Contain(new Player("O"));
        state.PlayManager.ActivePlayers.Should().Contain(new Player("X")).And.Subject.Should().Contain(new Player("O"));
    }

    [Fact]
    public void Round_RoundIndexStartsAtZero() {
        var state = new TicTacToeState(
            [new Player("X"), new Player("O")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: false
        );
        state.PlayManager.RoundIndex.Should().Be(0);
    }

    [Fact]
    public void EndTurn_AdvancesTurn() {
        var state = new TicTacToeState(
            [new Player("X"), new Player("O")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: false
        );

        state.PlayManager.EndTurn("X", out _);
        state.PlayManager.ActivePlayers.Should().Contain(new Player("O"));
    }

    [Fact]
    public void EndTurn_EndRound_TracksRoundIndex() {
        var state = new TicTacToeState(
            [new Player("X"), new Player("O")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: false
        );

        // X, O both take their turns
        state.PlayManager.EndTurn("X", out _);
        state.PlayManager.EndTurn("O", out _);
        state.PlayManager.IsRoundOver.Should().BeTrue();
        state.PlayManager.GameStateText.Should().Be("Round over.");
    }

    [Fact]
    public void RoundComplete_OnePlayerResigned() {
        var state = new TicTacToeState(
            [new Player("X"), new Player("O")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: false
        );

        state.PlayManager.ResignPlayer("X");
        state.PlayManager.EndTurn("O", out _);

        state.PlayManager.ActivePlayers.Count().Should().Be(1);
        state.PlayManager.IsRoundOver.Should().BeTrue();
        state.PlayManager.GameStateText.Should().Be("Round over.");
    }

    [Fact]
    public void RoundComplete_TwoPlayers() {
        var state = new TicTacToeState(
            [new Player("A"), new Player("B")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: true
        );

        state.PlayManager.EndTurn("A", out _);
        state.PlayManager.EndTurn("B", out _);
        state.PlayManager.IsRoundOver.Should().BeTrue();
        state.PlayManager.GameStateText.Should().Be("Synchronized play.");
    }

    [Fact]
    public void RoundComplete_ThreePlayers() {
        var state = new TicTacToeState(
            [new Player("A"), new Player("B"), new Player("C")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: true
        );

        state.PlayManager.NumberOfActivePlayers.Should().Be(3);

        state.PlayManager.EndTurn("A", out _);
        state.PlayManager.EndTurn("B", out _);
        state.PlayManager.EndTurn("C", out _);
        state.PlayManager.IsRoundOver.Should().BeTrue();
        state.PlayManager.GameStateText.Should().Be("Synchronized play.");
    }

    [Fact]
    public void ResignPlayer_OnlyNextPlayerCanTakeTurn() {
        var state = new TicTacToeState(
            [new Player("A"), new Player("B"), new Player("C")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: false
        );

        state.PlayManager.ResignPlayer("A");

        state.PlayManager.CanTakeTurn(new Player("A")).Should().BeFalse();
        state.PlayManager.CanTakeTurn(new Player("B")).Should().BeTrue();
        state.PlayManager.CanTakeTurn(new Player("C")).Should().BeFalse();
        state.PlayManager.CanTakeTurn(new Player("D")).Should().BeFalse();
    }

    [Fact]
    public void ActivePlayers_ExcludesResignedPlayers() {
        var state = new TicTacToeState(
            [new Player("A"), new Player("B"), new Player("C")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: false
        );

        state.PlayManager.ResignPlayer("A");
        state.PlayManager.ActivePlayers.Should().Contain(new Player("B")).And.Subject.Should().Contain(new Player("C"));
    }

    [Fact]
    public void ResignPlayer_AddsToResignedPlayersSet() {
        var state = new TicTacToeState(
            [new Player("A"), new Player("B"), new Player("C")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: false
        );

        state.PlayManager.ResignPlayer("A");
        state.PlayManager.ResignedPlayersSet.Should().Contain("A");
    }

    [Fact]
    public void ResignPlayer_SkipsResignedTurn() {
        var state = new TicTacToeState(
            [new Player("A"), new Player("B"), new Player("C")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: false
        );

        state.PlayManager.ResignPlayer("A");
        state.PlayManager.EndTurn("A", out _); // A was resigned, turn skipped

        state.PlayManager.ActivePlayers.First().Should().Be(new Player("B"));
        state.PlayManager.EndTurn("B", out _);
        state.PlayManager.EndTurn("C", out _);
    }

    [Fact]
    public void Constructor_3Players_FirstPlayerIs_A() {
        var state = new TicTacToeState(
            [new Player("A"), new Player("B"), new Player("C")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: false
        );

        state.PlayManager.ActivePlayers.First().Should().Be(new Player("A"));
    }

    [Fact]
    public void Constructor_RandomPlayer() {
        var state = new TicTacToeState(
            [new Player("A"), new Player("B"), new Player("C")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: true,
            isSynchronousMode: false
        );
        var firstPlayer = state.PlayManager.ActivePlayers.First();
        var expectedPlayers = new[] { "A", "B", "C" };
        expectedPlayers.Contains(firstPlayer).Should().BeTrue();
    }


    [Fact]
    public void CanTakeTurn_AllAvailablePlayersSynchronousMode() {
        var state = new TicTacToeState(
            [new Player("A"), new Player("B"), new Player("C")],
            [new BoardBuilder(3, 3)],
            isRandomPlayerOrder: false,
            isSynchronousMode: true
        );

        state.PlayManager.ResignPlayer("A");
        
        // A has resigned, so A should be unable to take turns
        state.PlayManager.CanTakeTurn(new Player("A")).Should().BeFalse();
        
        // B is next to take a turn
        state.PlayManager.CanTakeTurn(new Player("B")).Should().BeTrue();
        state.PlayManager.CanTakeTurn(new Player("C")).Should().BeTrue();
        state.PlayManager.CanTakeTurn(new Player("D")).Should().BeFalse();
    }

}
