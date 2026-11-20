using MnkeyFog.Model.Template;
using MnkeyFog.Model.Views;

namespace MnkeyFog.Model.PlayerAIs;

[ModelSerializable]
public abstract class MonteCarloAI : IPlayerAI {

    //currently depth > 4 has dire performance.
    public abstract string Description { get; }
    public abstract int MaxDepth { get; }
    Random _random = new Random();

    public void Attempt(GameView gameView) {
        var optimalGameAction = FindOptimalGameAction(gameView);
        if (optimalGameAction != null) {
            gameView.Attempt(optimalGameAction);
        }
    }

    public GameAction? FindOptimalGameAction(GameView gameView) {
        ArgumentNullException.ThrowIfNull(gameView.PlayerIndex, $"{nameof(gameView)}.{nameof(gameView.PlayerIndex)}");
        var simulatedState = CloneGameState(gameView);
        var depth = 0;
        var optimalActionAssessment = SimulateTurn(simulatedState, gameView.PlayerIndex.Value, gameView.PlayerIndex.Value, depth);
        return optimalActionAssessment?.GameAction;
    }

    private ActionAssessment? SimulateTurn(
        GameState simulatedState,
        int currentPlayerIndex,
        int objectivePlayerIndex,
        int depth
    ) {
        var actionFactories = simulatedState.GameTemplate.GetAvailableActions(simulatedState, currentPlayerIndex);
        var actionAssessments = new List<ActionAssessment>();

        foreach (var factory in actionFactories) {
            GameAction? action = null;
            if (factory is GameActionFactoryForSimple factoryForSimple) {
                action = factoryForSimple.Create();
                actionAssessments.Add(
                    SimulateAction(CloneGameState(simulatedState), action, currentPlayerIndex, objectivePlayerIndex)
                );
            }
            if (factory is GameActionFactoryForSpace factoryForSpace) {
                for (sbyte boardIndex = 0; boardIndex < simulatedState.Boards.Count; boardIndex += 1) {
                    var board = simulatedState.GetBoardByIndex(boardIndex);
                    foreach (var spaceEnumerator in board.AsSpaceEnumerable()) {
                        if (spaceEnumerator.Space.MarkIndex == null) {
                            action = factoryForSpace.Create(boardIndex, spaceEnumerator.Col, spaceEnumerator.Row);
                            actionAssessments.Add(
                                SimulateAction(CloneGameState(simulatedState), action, currentPlayerIndex, objectivePlayerIndex)
                            );
                        }
                    }
                }
            }
            if (factory is GameActionFactoryForBoard factoryForBoard) {
                for (sbyte boardIndex = 0; boardIndex < simulatedState.Boards.Count; boardIndex += 1) {
                    var board = simulatedState.GetBoardByIndex(boardIndex);
                    foreach (var spaceEnumerator in board.AsSpaceEnumerable()) {
                        if (spaceEnumerator.Space.MarkIndex == null) {
                            action = factoryForBoard.Create(boardIndex);
                            actionAssessments.Add(
                                SimulateAction(CloneGameState(simulatedState), action, currentPlayerIndex, objectivePlayerIndex)
                            );
                        }
                    }
                }
            }
            if (factory is GameActionFactoryForRow) {
                //TODO GameActionFactoryForRow
                throw new NotImplementedException();
            }
            if (factory is GameActionFactoryForColumn) {
                //TODO GameActionFactoryForColumn
                throw new NotImplementedException();
            }
        }

        foreach (var actionAssessment in actionAssessments) {
            if (!actionAssessment.SimulatedState.IsGameOver && depth < MaxDepth) {
                var nextPlayer = actionAssessment.SimulatedState.PlayersState.PlayersAvailableForTurn.First();
                var recursiveActionAssessment = SimulateTurn(actionAssessment.SimulatedState, nextPlayer.Index, objectivePlayerIndex, depth + 1);
                if (recursiveActionAssessment != null) {
                    actionAssessment.Rating += recursiveActionAssessment.Rating;
                }
            }
        }

        var optimalActionAssessments = (
            (currentPlayerIndex == objectivePlayerIndex)
                ? actionAssessments.AllMaxBy(assessment => assessment.Rating)
                : actionAssessments.AllMinBy(assessment => assessment.Rating)
        ).ToList();

        //given a tie, choose randomly.
        var optimalActionAssessment = optimalActionAssessments.Count == 1
            ? optimalActionAssessments.Single()
            : optimalActionAssessments[_random.Next(0, optimalActionAssessments.Count)];

        return optimalActionAssessment;
    }

    private ActionAssessment SimulateAction(
        GameState simulatedState,
        GameAction gameAction,
        int currentPlayerIndex,
        int objectivePlayerIndex
    ) {
        var baseRating = RateScore(simulatedState.ScoreCard, objectivePlayerIndex);
        simulatedState.Attempt(new PlayerAction(gameAction, currentPlayerIndex));
        if (simulatedState.PlayersState.IsRoundOver) {
            simulatedState.EndRound(out _);
        }
        var newRating = RateScore(simulatedState.ScoreCard, objectivePlayerIndex);
        var resultRating = newRating - baseRating;
        return new ActionAssessment(simulatedState, gameAction, resultRating);
    }

    private GameState CloneGameState(GameView originalStateView) {
        ArgumentNullException.ThrowIfNull(originalStateView.PlayerIndex, $"{nameof(originalStateView)}.{nameof(originalStateView.PlayerIndex)}");
        var playersState = new PlayersState(originalStateView.PlayersState);
        var gameTemplate = originalStateView.GameTemplate;

        // Create a fresh copy of the game state
        var clonedState = new GameState(playersState, gameTemplate);
        foreach (var boardView in originalStateView.Boards) {
            foreach (var spaceView in boardView.AsSpaceViewEnumerable()) {
                var clonedSpace = clonedState.Boards[boardView.BoardIndex].Spaces[spaceView.Col, spaceView.Row];
                clonedSpace.MarkIndex = boardView.SpaceDTOs[spaceView.Col, spaceView.Row].MarkIndex;
                clonedSpace.MakeKnownToPlayerIndex(originalStateView.PlayerIndex.Value);
                if (clonedSpace.MarkIndex != null && clonedSpace.MarkIndex != Space.ImpasseMarkIndex) {
                    clonedSpace.MakeKnownToPlayerIndex(clonedSpace.MarkIndex.Value);
                }
            }
        }

        return clonedState;
    }

    private GameState CloneGameState(GameState originalState)
    => new GameState(originalState);

    public int RateScore(ScoreCard scoreCard, int playerIndex)
    => scoreCard.PlayerScores.SingleOrDefault(ps => ps.PlayerIndex == playerIndex).Score //playerscore is a struct so default score = 0;
    - scoreCard.PlayerScores.Where(ps => ps.PlayerIndex != playerIndex).Max(ps => ps.Score);
}

internal class ActionAssessment {
    public ActionAssessment(GameState simulatedState, GameAction gameAction, int rating) {
        SimulatedState = simulatedState;
        GameAction = gameAction;
        Rating = rating;
    }
    public GameState SimulatedState;
    public GameAction GameAction;
    public int Rating;
}
