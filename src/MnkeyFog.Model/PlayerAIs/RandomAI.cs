using MnkeyFog.Model.Template;
using MnkeyFog.Model.Views;

namespace MnkeyFog.Model.PlayerAIs;

[ModelSerializable]
public class RandomAI : IPlayerAI {
    public string Description => "Randy, Difficulty 1";
    public void Attempt(GameView gameView) {
        var actionFactories = gameView.AvailableActions;
        var actionFactory = SelectRandomItem(actionFactories);
        if (actionFactory is GameActionFactoryForSimple actionFactoryForSimple) {
            gameView.Attempt(actionFactoryForSimple.Create());
        } else if (actionFactory is GameActionFactoryForBoard actionFactorForBoard) {
            var board = SelectRandomItem(gameView.Boards);
            gameView.Attempt(actionFactorForBoard.Create(board.BoardIndex));
        } else if (actionFactory is GameActionFactoryForSpace actionFactoryForSpace) {
            var spaceView = SelectRandomItem(gameView.SpaceViews);
            gameView.Attempt(actionFactoryForSpace.Create(spaceView.BoardIndex, spaceView.Col, spaceView.Row));
        }
    }

    private static T SelectRandomItem<T>(IEnumerable<T> items) {
        var list = items.ToList();
        return list[Random.Shared.Next(list.Count)];
    }
}
