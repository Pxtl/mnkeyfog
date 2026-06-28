using KriegSpielTicTacToe.Model.Template; using KriegSpielTicTacToe.Modelsproj.Views;

namespace KriegSpielTicTacToe.Model.PlayerAIs { 
    [ModelSerializable] public class WinnerAI : IPlayerAI {
        public string Description => "Winner, Difficulty 3";

        public void Attempt(GameView gameView, IEnumerable<GameActionFactory> actionFactories) {
            // Check if already a winner before attempting additional moves
            var scoreCard = gameView.ScoreCard;
            foreach (var winner in scoreCard.Highest.Players) {
                if (winner.Mark == gameView.Player!.Mark) return;  // Already won, don't play more
            }

            var factorySpaceActions = new List<GameActionFactoryForSpace>();
            foreach (var sa in actionFactories.OfType<GameActionFactoryForSpace>()) {
                if (!factorySpaceActions.Contains(sa)) factorySpaceActions.Add(sa);
            }

            if (factorySpaceActions.Count == 0) return;

            foreach (var board in gameView.Boards) {
                sbyte rc = (sbyte)board.RowCount;
                sbyte cc = (sbyte)board.ColumnCount;
                
                if (rc <= 0 || cc <= 0) continue;

                for (int row = 0; row < rc; row++) {
                    for (int col = 0; col < cc; col++) {
                        string spaceName = $"Board#{board.BoardIndex},{row+1},{col+1}";
                        
                        if (!gameView.TryGetCoordinatesFromSpaceName(spaceName, out sbyte bi, 
                                out sbyte r, out sbyte c) || row != r || col != c) continue;

                        try { gameView.Attempt(factorySpaceActions[0].Create(bi, (sbyte)col, (sbyte)row)); return; } catch { }
                    }
                }
            }
        }
    }
}
