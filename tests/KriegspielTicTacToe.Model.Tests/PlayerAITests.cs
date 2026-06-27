using KriegspielTicTacToe.Model.PlayerAIs;

namespace KriegspielTicTacToe.Model.Tests;

public class PlayerAITests {
    [Fact]
    public void AIGameRunner_GameEnds() {
        var gameTemplate = GameTemplates.Weinersmith;
        var playerAIs = new OrderedDictionary<Player, IPlayerAI> {
            [new Player("X")] = new RandomAI(),
            [new Player("O")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(gameTemplate, playerAIs);
        };
        action.Should().NotThrow();
    }
}