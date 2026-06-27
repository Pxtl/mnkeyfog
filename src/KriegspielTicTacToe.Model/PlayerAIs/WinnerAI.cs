using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.Views;

namespace KriegspielTicTacToe.Model.PlayerAIs;

[ModelSerializable]
public class WinnerAI : IPlayerAI {
    public string Description => "Winner, Difficulty 3";

    public void Attempt(GameView gameView, IEnumerable<GameActionFactory> actionFactories)
    {
        var spaceActions = actionFactories.OfType<GameActionFactoryForSpace>().ToList();
        var simpleActions = actionFactories.OfType<GameActionFactoryForSimple>().ToList();
        var availableSpaceNames = gameView.SpaceNames.ToList();

        // For 3x3 small boards, try smart positions: center "5", then corners/edges  
        bool isSmallBoard = availableSpaceNames.Count == 9 && 
            allNumeric(availableSpaceNames);

        if (isSmallBoard)
        {
            foreach (var spaceName in new[] { "5", "1", "3", "7", "9", "2", "4", "6", "8" })
            {
                var factory = spaceActions.FirstOrDefault(f => f is GameActionFactoryForSpace);
                if (factory != null)
                {
                    gameView.TryGetCoordinatesFromSpaceName(spaceName, out sbyte boardIdx, out var col, out var row);
                    gameView.Attempt(factory.Create(boardIdx, col, row));
                    return;
                }

                // Simple action fallback
                var simple = simpleActions.FirstOrDefault();
                if (simple != null)
                {
                    gameView.Attempt(simple.Create());
                    return;
                }
            }
        }
        else
        {
            // Large board or multi-board: play first available space action
            if (spaceActions.Any())
            {
                var factory = spaceActions.First();
                foreach (var space in availableSpaceNames)
                {
                    gameView.TryGetCoordinatesFromSpaceName(space, out sbyte _, out _, out _);
                    gameView.Attempt(factory.Create(0, 0, 0));
                    return;
                }

                // Fallback: simple action if no valid space found
                var simple = simpleActions.FirstOrDefault();
                if (simple != null)
                {
                    gameView.Attempt(simple.Create());
                }
            }
        }
    }

    private static bool allNumeric(IEnumerable<string> strings) =>
        strings.All(s => int.TryParse(s, out _));
}
