using MnkeyFog.Model.PlayerAIs;

namespace MnkeyFog.Model.Tests;

public class PlayerAITests {
    [Fact]
    public void AIGameRunner_BasicTicTacToeGameEnds() {
        var playerAIs = new OrderedDictionary<PlayerInfo, IPlayerAI> {
            [new PlayerInfo("X")] = new RandomAI(),
            [new PlayerInfo("O")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.BasicTicTacToe, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_KriegspielTicTacToeGameEnds() {
        var playerAIs = new OrderedDictionary<PlayerInfo, IPlayerAI> {
            [new PlayerInfo("X")] = new RandomAI(),
            [new PlayerInfo("O")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.KriegspielTicTacToe, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_Match3GameEnds() {
        var playerAIs = new OrderedDictionary<PlayerInfo, IPlayerAI> {
            [new PlayerInfo("X")] = new RandomAI(),
            [new PlayerInfo("O")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.Match3, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_FreestyleGomokuEnds() {
        var playerAIs = new OrderedDictionary<PlayerInfo, IPlayerAI> {
            [new PlayerInfo("X")] = new RandomAI(),
            [new PlayerInfo("O")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.FreestyleGomoku, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_FogGomokuEnds() {
        var playerAIs = new OrderedDictionary<PlayerInfo, IPlayerAI> {
            [new PlayerInfo("A")] = new RandomAI(),
            [new PlayerInfo("B")] = new RandomAI(),
            [new PlayerInfo("C")] = new RandomAI(),
            [new PlayerInfo("D")] = new RandomAI(),
            [new PlayerInfo("E")] = new RandomAI(),
            [new PlayerInfo("F")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.FogGomoku, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_FogTicTacToeEnds() {
        var playerAIs = new OrderedDictionary<PlayerInfo, IPlayerAI> {
            [new PlayerInfo("A")] = new RandomAI(),
            [new PlayerInfo("B")] = new RandomAI(),
            [new PlayerInfo("C")] = new RandomAI(),
            [new PlayerInfo("D")] = new RandomAI()
        };
        var action = () => {
            AIGameRunner.RunAIGame(GameTemplates.FogTicTacToe, playerAIs);
        };
        action.Should().NotThrow();
    }

    [Fact]
    public void AIGameRunner_BasicTicTacToe_AsterAIvsRandom() {
        // AsterAI vs RandomAI should show AsterAI winning more often
        int iterations = 50;

        const int asterAiSeed = 100;
        const int randomAiSeed = 200;
        var playerAIs = new OrderedDictionary<PlayerInfo, IPlayerAI> {
            [new PlayerInfo("X")] = new AsterAI(asterAiSeed),
            [new PlayerInfo("O")] = new RandomAI(randomAiSeed)
        };
        var asterAIPlayerXIndex = 0;
        var scoreSum = ScoreCard.Empty;
        for (int i = 0; i < iterations; i++) {
            scoreSum += AIGameRunner.RunAIGame(GameTemplates.BasicTicTacToe, playerAIs, out var gameState, isRandomPlayerOrder: false);
            // commented out debugging code because it was too much content.
            // Console.Out.WriteLine(new BoardRenderer().DrawBoards(gameState.GetSpectatorView(), 100));
            // Console.Out.WriteLine(gameState.GameStateText);
        }
        scoreSum.Highest.PlayerScores.Count().Should().Be(1);
        scoreSum.Highest.PlayerScores.Single().PlayerIndex.Should().Be(asterAIPlayerXIndex);
    }

    [Fact]
    public void AIGameRunner_BasicTicTacToe_MontyAIvsAsterAI() {
        // MontyAI vs AsterAI should show MontyAI winning more often
        int iterations = 50;

        const int montyAiSeed = 500;
        const int asterAiSeed = 600;
        var playerAIs = new OrderedDictionary<PlayerInfo, IPlayerAI> {
            [new PlayerInfo("X")] = new MontyAI(montyAiSeed),
            [new PlayerInfo("O")] = new AsterAI(asterAiSeed)
        };
        var montyAIPlayerXIndex = 0;
        var scoreSum = ScoreCard.Empty;
        for (int i = 0; i < iterations; i++) {
            scoreSum += AIGameRunner.RunAIGame(GameTemplates.BasicTicTacToe, playerAIs, out var gameState, isRandomPlayerOrder: false);
            // commented out debugging code because it was too much content.
            // Console.Out.WriteLine(new BoardRenderer().DrawBoards(gameState.GetSpectatorView(), 100));
            // Console.Out.WriteLine(gameState.GameStateText);
        }
        scoreSum.Highest.PlayerScores.Count().Should().Be(1);
        scoreSum.Highest.PlayerScores.Single().PlayerIndex.Should().Be(montyAIPlayerXIndex);
    }

    [Fact]
    public void MontyAI_KnowsToBlock() {
        var playerX = new Player("X", 0);
        var playerO = new Player("O", 1);
        var gameState = new GameState([playerX.Info, playerO.Info], GameTemplates.BasicTicTacToe, isRandomPlayerOrder: false);
        gameState.Boards[0].Spaces[1, 1].MarkIndex = playerO.Index;
        gameState.Boards[0].Spaces[2, 0].MarkIndex = playerO.Index;
        gameState.Boards[0].Spaces[2, 2].MarkIndex = playerX.Index;
        gameState.Boards[0].Spaces[2, 1].MarkIndex = playerX.Index;
        var gameView = gameState.GetView(playerX);
        var montyAI = new MontyAI(700);
        var gameAction = montyAI.FindOptimalGameAction(gameView)!;
        gameAction.GetType().Should().Be(typeof(MNKAction));
        var mnkAction = (MNKAction)gameAction;
        mnkAction.BoardIndex.Should().Be(0);
        mnkAction.Col.Should().Be(0);
        mnkAction.Row.Should().Be(2);
    }

    [Fact]
    public void AIGameRunner_FogTicTacToe_AsterAIvsRandom() {
        // AsterAI vs RandomAI should show AsterAI winning more often
        int iterations = 50;

        const int asterAiSeed = 300;
        const int randomAiSeed = 400;
        var playerAIs = new OrderedDictionary<PlayerInfo, IPlayerAI> {
            [new PlayerInfo("X")] = new AsterAI(asterAiSeed),
            [new PlayerInfo("O")] = new RandomAI(randomAiSeed)
        };
        var playerState = new PlayersState(playerAIs.Keys, RoundRobinPlayManager.Instance);
        var asterAIPlayerX = playerAIs.Keys.First();
        var scoreSum = ScoreCard.Empty;
        for (int i = 0; i < iterations; i++) {
            var result = AIGameRunner.RunAIGame(GameTemplates.FogTicTacToe, playerAIs, out var gameState, isRandomPlayerOrder: false);
            // add 1 point per-win.
            scoreSum += new ScoreCard(
                result.Highest.PlayerScores.Select(ps => new PlayerIndexScore(ps.PlayerIndex, 1))
            );
            // commented out debugging code because it was too much content.
            // Console.Out.WriteLine(new BoardRenderer().DrawBoards(gameState.GetSpectatorView(), 100));
            // Console.Out.WriteLine(gameState.GameStateText);
        }
        scoreSum.Highest.AsPlayerInfos(playerState).Count().Should().Be(1);
        scoreSum.Highest.AsPlayerInfos(playerState).Single().Should().Be(asterAIPlayerX);
    }
}
