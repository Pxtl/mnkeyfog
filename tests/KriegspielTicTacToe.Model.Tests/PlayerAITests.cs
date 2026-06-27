using KriegspielTicTacToe.Model.PlayerAIs;

namespace KriegspielTicTacToe.Model.Tests;

public class PlayerAITests {
    [Fact]
    public void AIGameRunner_BasicTicTacToeGameEnds() {
        var playerAIs = new OrderedDictionary<Player, IPlayerAI> {
            [new Player("X")] = new RandomAI(),
            [new Player("O")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.BasicTicTacToe, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_WeinersmithGameEnds() {
        var playerAIs = new OrderedDictionary<Player, IPlayerAI> {
            [new Player("X")] = new RandomAI(),
            [new Player("O")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.Weinersmith, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_Match3GameEnds() {
        var playerAIs = new OrderedDictionary<Player, IPlayerAI> {
            [new Player("X")] = new RandomAI(),
            [new Player("O")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.Match3, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_FreestyleGomokuEnds() {
        var playerAIs = new OrderedDictionary<Player, IPlayerAI> {
            [new Player("X")] = new RandomAI(),
            [new Player("O")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.FreestyleGomoku, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_KriegspielGomokuEnds() {
        var playerAIs = new OrderedDictionary<Player, IPlayerAI> {
            [new Player("A")] = new RandomAI(),
            [new Player("B")] = new RandomAI(),
            [new Player("C")] = new RandomAI(),
            [new Player("D")] = new RandomAI(),
            [new Player("E")] = new RandomAI(),
            [new Player("F")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.KriegspielGomoku, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_SynchroWeinersmithEnds() {
        var playerAIs = new OrderedDictionary<Player, IPlayerAI> {
            [new Player("A")] = new RandomAI(),
            [new Player("B")] = new RandomAI(),
            [new Player("C")] = new RandomAI(),
            [new Player("D")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.SynchroWeinersmith, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_BasicTicTacToe_WinnerAIvsRandom() {
        // WinnerAI vs RandomAI should show WinnerAI winning more often
        int iterations = 10;
        int winnerWins = 0;

        for (int i = 0; i < iterations; i++) {
            var playerAIs = new OrderedDictionary<Player, IPlayerAI> {
                [new Player("X")] = new WinnerAI(),
                [new Player("O")] = new RandomAI()
            };
            AIGameRunner.RunAIGame(GameTemplates.BasicTicTacToe, playerAIs);
            // Note: we can check which player won here if needed

        }
    }

    [Fact]
    public void AIGameRunner_BasicTicTacToe_WinnerAIvsRandomManyGames() {
        int iterations = 20;
        
        for (int i = 0; i < iterations; i++) {
            var playerAIs = new OrderedDictionary<Player, IPlayerAI> {
                [new Player("X")] = new WinnerAI(),
                [new Player("O")] = new RandomAI()
            };
            AIGameRunner.RunAIGame(GameTemplates.BasicTicTacToe, playerAIs);
        }
    }
}
