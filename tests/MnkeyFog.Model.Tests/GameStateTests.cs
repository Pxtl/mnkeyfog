namespace MnkeyFog.Model.Tests;

public class GameStateTests {
    [Fact]
    public void Constructor_EmptyBoards() {
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate(Array.Empty<BoardBuilder>(), isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        var playerX = state.PlayersState.GetPlayer("X");
        var playerO = state.PlayersState.GetPlayer("O");
        state.Boards.Should().BeEmpty();
        state.PlayersState.PlayerInfos.Should().Contain(new PlayerInfo("X"))
            .And.Subject.Should().Contain(new PlayerInfo("O"));
        state.PlayersState.ActivePlayers.Should().Contain(playerX)
            .And.Subject.Should().Contain(playerO);
    }

    [Fact]
    public void NoScore_GameStateSaysNobodyWins() {
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate(
                [new BoardBuilder(3, 3, new MNKBoardRuleset(ScoringLength: null, IsBoardDoneWhenScored: true))],
                isSynchronousMode: false,
                isKriegspiel: false
            ),
            isRandomPlayerOrder: false
        );
        var playerXView = state.GetView(state.PlayersState.GetPlayer("X"));
        var playerOView = state.GetView(state.PlayersState.GetPlayer("O"));
        //XXO
        //OOX
        //XOX
        playerXView.Attempt(new MNKAction(0, 0, 0));
        playerOView.Attempt(new MNKAction(0, 0, 1));
        state.EndRound(out _);
        playerXView.Attempt(new MNKAction(0, 1, 0));
        playerOView.Attempt(new MNKAction(0, 1, 1));
        state.EndRound(out _);
        playerXView.Attempt(new MNKAction(0, 0, 2));
        playerOView.Attempt(new MNKAction(0, 2, 0));
        state.EndRound(out _);
        playerXView.Attempt(new MNKAction(0, 2, 1));
        playerOView.Attempt(new MNKAction(0, 1, 2));
        state.EndRound(out _);
        playerXView.Attempt(new MNKAction(0, 2, 2));
        state.Winners.Should().Contain(playerXView.PlayerInfo!);
        state.Winners.Should().Contain(playerOView.PlayerInfo!);
        state.GameStateText.Should().Contain("Nobody wins.");
    }

    [Fact]
    public void TieGame_GameStateSaysTheyBothWin() {
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate(
                [new BoardBuilder(3, 3, new MNKBoardRuleset(ScoringLength: null, IsBoardDoneWhenScored: true))],
                isSynchronousMode: true,
                isKriegspiel: false
            ),
            isRandomPlayerOrder: false
        );

        //XXX
        //OIX
        //OOO
        var playerXView = state.GetView(state.PlayersState.GetPlayer("X"));
        var playerOView = state.GetView(state.PlayersState.GetPlayer("O"));
        var result = playerXView.Attempt(new MNKAction(0, 0, 0));
        result = playerOView.Attempt(new MNKAction(0, 0, 2));
        state.EndRound(out _);

        playerXView = state.GetView(state.PlayersState.GetPlayer("X"));
        playerOView = state.GetView(state.PlayersState.GetPlayer("O"));
        result = playerXView.Attempt(new MNKAction(0, 1, 0));
        result = playerOView.Attempt(new MNKAction(0, 1, 2));
        state.EndRound(out _);

        playerXView = state.GetView(state.PlayersState.GetPlayer("X"));
        playerOView = state.GetView(state.PlayersState.GetPlayer("O"));
        result = playerXView.Attempt(new MNKAction(0, 2, 0));
        result = playerOView.Attempt(new MNKAction(0, 2, 2));
        state.EndRound(out _);

        playerXView = state.GetView(state.PlayersState.GetPlayer("X"));
        playerOView = state.GetView(state.PlayersState.GetPlayer("O"));
        result = playerXView.Attempt(new MNKAction(0, 1, 2));
        result = playerOView.Attempt(new MNKAction(0, 1, 0));
        state.EndRound(out _);

        playerXView = state.GetView(state.PlayersState.GetPlayer("X"));
        playerOView = state.GetView(state.PlayersState.GetPlayer("O"));
        result = playerXView.Attempt(new MNKAction(0, 1, 1));
        result = playerOView.Attempt(new MNKAction(0, 1, 1));

        //DEBUG
        Console.WriteLine(new BoardRenderer().DrawBoards(state.GetSpectatorView(), doShowSpaceCommandName: false, maxRenderWidth: 120));
        Console.WriteLine(state.GameStateText);

        state.EndRound(out _);
        state.Winners.Should().Contain(playerXView.PlayerInfo!);
        state.Winners.Should().Contain(playerOView.PlayerInfo!);
        state.GameStateText.Should().Contain("X");
        state.GameStateText.Should().Contain("O");
        state.GameStateText.Should().Contain("win(s)");

    }
}
