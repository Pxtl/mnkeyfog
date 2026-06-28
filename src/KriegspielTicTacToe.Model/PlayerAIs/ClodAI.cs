using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.Views;

namespace KriegspielTicTacToe.Model.PlayerAIs {

    [ModelSerializable]
    public class ClodAI : IPlayerAI {
        public string Description => "Clod, Difficulty 2";

        public void Attempt(GameView gameView, IEnumerable<GameActionFactory> actionFactories) {
            var factorySpaceActions = new List<GameActionFactoryForSpace>(actionFactories.OfType<GameActionFactoryForSpace>());
            var simpleFactory = actionFactories.OfType<GameActionFactoryForSimple>().FirstOrDefault();

            // Iterate ALL boards - handles multi-board games correctly
            foreach (var board in gameView.Boards) {
                sbyte rc = board.RowCount;  // row count  
                sbyte cc = board.ColumnCount;
                
                // Calculate center: (count - 1) / 2, explicit casts throughout               
                double centerX = (cc - 1) / 2.0;      // force double arithmetic
                double centerY = (rc - 1) / 2.0;
                
                sbyte centerCol = (sbyte)Math.Floor(centerX);
                sbyte centerRow = (sbyte)Math.Floor(centerY);

                // Vertical scan through center column
                for (sbyte delta = -3; delta <= 3; delta++) {
                    sbyte testRow = ((sbyte)(centerRow + delta));   // sbyte + sbyte = sbyte
                    gameView.Attempt(factorySpaceActions[0].Create(board.BoardIndex, centerCol, testRow));
                }

                // Horizontal scan through center row
                for (sbyte delta = -3; delta <= 3; delta++) {
                    sbyte testCol = ((sbyte)(centerCol + delta));   
                    gameView.Attempt(factorySpaceActions[0].Create(board.BoardIndex, testCol, centerRow));
                }

                // Diagonal up-right through center 
                for (sbyte delta = -3; delta <= 3; delta++) {
                    sbyte diagCol = ((sbyte)(centerCol + delta));
                    sbyte diagRow = ((sbyte)(centerRow + delta));
                    gameView.Attempt(factorySpaceActions[0].Create(board.BoardIndex, diagCol, diagRow));
                }

                // Diagonal up-left through center
                for (sbyte delta = -3; delta <= 3; delta++) {
                    sbyte diagCol2 = ((sbyte)(centerCol + delta));
                    sbyte diagRow2 = ((sbyte)(centerRow - delta));
                    gameView.Attempt(factorySpaceActions[0].Create(board.BoardIndex, diagCol2, diagRow2));
                }
            }

            // SpaceNames fallback: try each space name for coordinate lookup
            foreach (var spaceName in gameView.SpaceNames) {
                bool result = gameView.TryGetCoordinatesFromSpaceName(spaceName, out sbyte biBox, out sbyte colBox2, out sbyte rowBox); 
                               
                if(factorySpaceActions.Count > 0) {
                    gameView.Attempt(factorySpaceActions[0].Create(biBox, colBox2, rowBox));
                }
            }

            // Final fallback: simple action
            if (simpleFactory != null) gameView.Attempt(simpleFactory.Create());
        }
    }
}
