namespace KriegspielTicTacToe.Model.Tests;

public class PlayManagerTests {
    #region unique player marks
    [Fact]
    public void RoundRobinPlayManagerConstructor_WithUniqueMarksIsAllowed() {
        var expectedPlayers = new List<Player>() { new("X"), new("O") };
        var actualManager = new RoundRobinPlayManager(expectedPlayers);
        actualManager.Players.Should().BeEquivalentTo(expectedPlayers);
    }

    [Fact]
    public void RoundRobinPlayManagerConstructor_WithNonUniqueMarksThrows() {
        var expectedPlayers = new List<Player>() { new("X"), new("O"), new("X") };
        var action = () => {
            _ = new RoundRobinPlayManager(expectedPlayers);
        };
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RoundRobinPlayManagerConstructor_WithMarksSameButDifferentCaseThrows() {
        var expectedPlayers = new List<Player>() { new("X"), new("O"), new("x") };
        var action = () => {
            _ = new RoundRobinPlayManager(expectedPlayers);
        };
        action.Should().Throw<ArgumentException>();
    }
    #endregion

    [Fact]
    public void GameStateConstructor_WithBoardsCreatesProperState() {
        var state = new GameState(
            [new Player("X"), new Player("O")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3), MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        state.Boards.Count.Should().Be(2);
        state.PlayManager.Players.Should().Contain(new Player("X"));
        state.PlayManager.Players.Should().Contain(new Player("O"));
        state.PlayManager.ActivePlayers.Should().Contain(new Player("X"));
        state.PlayManager.ActivePlayers.Should().Contain(new Player("O"));
    }

    [Fact]
    public void Round_RoundIndexStartsAtZero() {
        var state = new GameState(
            [new Player("X"), new Player("O")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        state.PlayManager.RoundIndex.Should().Be(0);
    }

    [Fact]
    public void EndTurn_AdvancesTurn() {
        var state = new GameState(
            [new Player("X"), new Player("O")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        state.PlayManager.EndTurn(new Player("X"), out _);
        state.PlayManager.ActivePlayers.Should().Contain(new Player("O"));
    }

    [Fact]
    public void EndTurn_EndRound_TracksRoundIndex() {
        var state = new GameState(
            [new Player("X"), new Player("O")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        state.PlayManager.EndTurn(new Player("X"), out _);
        state.PlayManager.EndTurn(new Player("O"), out _);
        state.PlayManager.IsRoundOver.Should().BeTrue();
        state.PlayManager.GameStateText.Should().Be("Round over.");

        state.PlayManager.EndRound(out _);
        state.PlayManager.RoundIndex.Should().Be(1);
    }

    [Fact]
    public void RoundComplete_OnePlayerResigned() {
        var state = new GameState(
            [new Player("X"), new Player("O")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        state.PlayManager.ResignPlayer(new Player("X"));
        state.PlayManager.EndTurn(new Player("O"), out _);

        state.PlayManager.ActivePlayers.Count().Should().Be(1);
        state.PlayManager.IsRoundOver.Should().BeTrue();
        state.PlayManager.GameStateText.Should().Be("Round over.");
    }

    [Fact]
    public void RoundComplete_TwoPlayers() {
        var state = new GameState(
            [new Player("A"), new Player("B")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: true, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        state.PlayManager.EndTurn(new Player("A"), out _);
        state.PlayManager.EndTurn(new Player("B"), out _);
        state.PlayManager.IsRoundOver.Should().BeTrue();
        state.PlayManager.GameStateText.Should().Be("Synchronized play. Round complete.");
    }

    [Fact]
    public void RoundComplete_ThreePlayers() {
        var state = new GameState(
            [new Player("A"), new Player("B"), new Player("C")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: true, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        state.PlayManager.NumberOfActivePlayers.Should().Be(3);

        state.PlayManager.EndTurn(new Player("A"), out _);
        state.PlayManager.EndTurn(new Player("B"), out _);
        state.PlayManager.EndTurn(new Player("C"), out _);
        state.PlayManager.IsRoundOver.Should().BeTrue();
        state.PlayManager.GameStateText.Should().Be("Synchronized play. Round complete.");
    }

    [Fact]
    public void ResignPlayerInRoundRobinMode_OnlyNextPlayerCanTakeTurn() {
        var state = new GameState(
            [new Player("A"), new Player("B"), new Player("C")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        state.PlayManager.ResignPlayer(new Player("A"));

        state.PlayManager.CanTakeTurn(new Player("A")).Should().BeFalse();
        state.PlayManager.CanTakeTurn(new Player("B")).Should().BeTrue();
        state.PlayManager.CanTakeTurn(new Player("C")).Should().BeFalse();
        state.PlayManager.CanTakeTurn(new Player("D")).Should().BeFalse();
    }

    [Fact]
    public void ActivePlayers_ExcludesResignedPlayers() {
        var state = new GameState(
            [new Player("A"), new Player("B"), new Player("C")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        state.PlayManager.ResignPlayer(new Player("A"));
        state.PlayManager.ActivePlayers.Should().Contain(new Player("B"));
        state.PlayManager.ActivePlayers.Should().Contain(new Player("C"));
    }

    [Fact]
    public void ResignPlayer_AddsToResignedPlayersSet() {
        var state = new GameState(
            [new Player("A"), new Player("B"), new Player("C")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        state.PlayManager.ResignPlayer(new Player("A"));
        state.PlayManager.ResignedPlayersSet.Should().Contain(new Player("A"));
    }

    [Fact]
    public void ResignPlayer_SkipsResignedTurn() {
        var state = new GameState(
            [new Player("A"), new Player("B"), new Player("C")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        state.PlayManager.ResignPlayer(new Player("A"));
        state.PlayManager.EndTurn(new Player("A"), out _); // A was resigned, turn skipped

        state.PlayManager.ActivePlayers.First().Should().Be(new Player("B"));
        state.PlayManager.EndTurn(new Player("B"), out _);
        state.PlayManager.EndTurn(new Player("C"), out _);
    }

    [Fact]
    public void GameStateConstructor_3Players_FirstPlayerIs_A() {
        var state = new GameState(
            [new Player("A"), new Player("B"), new Player("C")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        state.PlayManager.ActivePlayers.First().Should().Be(new Player("A"));
    }

    [Fact]
    public void GameStateConstructor_RandomPlayer() {
        var state = new GameState(
            [new Player("A"), new Player("B"), new Player("C")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: true
        );
        var firstPlayer = state.PlayManager.ActivePlayers.First();
        var expectedPlayers = new[] {new Player("A"), new Player("B"), new Player("C")};
        expectedPlayers.Contains(firstPlayer).Should().BeTrue();
    }

    [Fact]
    public void CanTakeTurn_AllAvailablePlayersSynchronousMode() {
        var state = new GameState(
            [new Player("A"), new Player("B"), new Player("C")],
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: true, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        state.PlayManager.ResignPlayer(new Player("A"));
        
        state.PlayManager.CanTakeTurn(new Player("A")).Should().BeFalse();
        state.PlayManager.CanTakeTurn(new Player("B")).Should().BeTrue();
        state.PlayManager.CanTakeTurn(new Player("C")).Should().BeTrue();
        
        // player D does not exist.
        state.PlayManager.CanTakeTurn(new Player("D")).Should().BeFalse();
    }

}
