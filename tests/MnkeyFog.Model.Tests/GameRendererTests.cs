namespace MnkeyFog.Model.Tests;

public class GameRendererTests {
    private static GameState MakeState() {
        var boardBuilder = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        var template = new MNKTemplate(
            "mnk",
            "Test game",
            new[] { 2, 3 },
            [boardBuilder],
            isKriegspiel: false,
            isSynchronousMode: false
        );
        return new GameState(
            new[] { 'X', 'O' }.ToPlayersArray(),
            template,
            isRandomPlayerOrder: false
        );
    }

    [Fact]
    public void Render_OrdersGameName_Event_Board_ActivePlayers() {
        var state = MakeState();
        var actual = new GameRenderer(new ModelNamer()).Render(new GameView(state, null));

        actual.Should().StartWith("mnk");
        actual.Should().Contain("Turn:");
        actual.Should().Contain("┌");
        actual.Should().Contain("Players:");

        // Order: game name < event message < board < active players.
        var gameNameIdx = actual.IndexOf("mnk", StringComparison.Ordinal);
        var turnIdx = actual.IndexOf("Turn:", StringComparison.Ordinal);
        var boardIdx = actual.IndexOf("┌", StringComparison.Ordinal);
        var playersIdx = actual.IndexOf("Players:", StringComparison.Ordinal);
        gameNameIdx.Should().BeLessThan(turnIdx);
        turnIdx.Should().BeLessThan(boardIdx);
        boardIdx.Should().BeLessThan(playersIdx);
    }

    [Fact]
    public void Render_ResignedPlayer_IsListedAsResigned() {
        var state = MakeState();
        state.ResignPlayer(1);
        var actual = new GameRenderer(new ModelNamer()).Render(new GameView(state, null));

        actual.Should().Contain("Resigned: O");
    }
}
