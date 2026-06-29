using KriegspielTicTacToe.Model.Template;
using KriegspielTicTacToe.Model.Views;

namespace KriegspielTicTacToe.Model.PlayerAIs {

    /// <summary>
    /// Aster is a human-fixed version of Clod.
    /// </summary>
    [ModelSerializable]
    public class AsterAI : IPlayerAI {
        public string Description => "Aster, Difficulty 3";

        // persist these across executions so we'll keep our order of plan from
        // one turn to next.
        List<BoardView>? Boards { get; set; } = null;
        List<List<(sbyte Col, sbyte Row)>>? Lines { get; set; } = null;

        public void Attempt(GameView gameView, IEnumerable<GameActionFactory> actionFactories) {
            var factorySpaceActions = new List<GameActionFactoryForSpace>(actionFactories.OfType<GameActionFactoryForSpace>());
            var simpleFactory = actionFactories.OfType<GameActionFactoryForSimple>().FirstOrDefault();

            //shuffle the boards so it won't be a consistent pattern of where it starts.
            if (Boards == null) {
                Boards = gameView.Boards.Shuffle().ToList();
            }

            // Iterate ALL boards - handles multi-board games correctly
            foreach (var board in Boards) {
                
                // Calculate center: (count - 1) / 2, explicit casts throughout               
                double centreX = (board.ColumnCount - 1) / 2.0;      // force double arithmetic
                double centreY = (board.RowCount - 1) / 2.0;
                
                var centreCol = (sbyte)centreX;
                var centreRow = (sbyte)centreY;

                if(Lines == null) {
                    Lines = new List<List<(sbyte Col, sbyte Row)>>();
                    var currentLine = new List<(sbyte Col, sbyte Row)>();

                    // Vertical scan through center column
                    currentLine.Clear();
                    currentLine.Add((centreCol, centreRow));
                    for (sbyte delta = 1; true; delta += 1) {
                        if ((centreRow + delta >= board.RowCount) && (centreRow - delta < 0)) {
                            break;
                        }
                        currentLine.Add((centreCol, (sbyte)(centreRow + delta)));
                        currentLine.Add((centreCol, (sbyte)(centreRow - delta)));                    
                    }
                    Lines.Add(currentLine);

                    // Horizontal scan through center row
                    currentLine.Clear();
                    currentLine.Add((centreCol, centreRow));
                    for (sbyte delta = 1; true; delta += 1) {
                        if ((centreCol + delta >= board.ColumnCount) && (centreCol - delta < 0)) {
                            break;
                        }
                        currentLine.Add(((sbyte)(centreCol + delta), centreRow));
                        currentLine.Add(((sbyte)(centreCol - delta), centreRow));          
                    }
                    Lines.Add(currentLine);

                    // Identity diagonal through centre point
                    currentLine.Clear();
                    currentLine.Add((centreCol, centreRow));
                    for (sbyte delta = 1; true; delta += 1) {
                        if (
                            (centreRow + delta >= board.RowCount) 
                            && (centreRow - delta < 0) 
                            && (centreCol + delta >= board.ColumnCount) 
                            && (centreCol - delta < 0)
                        ) {
                            break;
                        }
                        currentLine.Add(((sbyte)(centreCol + delta), (sbyte)(centreRow + delta)));
                        currentLine.Add(((sbyte)(centreCol - delta), (sbyte)(centreRow - delta)));
                    }
                    Lines.Add(currentLine);

                    // Inverse diagonal through centre point
                    currentLine.Clear();
                    currentLine.Add((centreCol, centreRow));
                    for (sbyte delta = 1; true; delta += 1) {
                        if (
                            (centreRow + delta >= board.RowCount) 
                            && (centreRow - delta < 0) 
                            && (centreCol + delta >= board.ColumnCount) 
                            && (centreCol - delta < 0)
                        ) {
                            break;
                        }
                        currentLine.Add(((sbyte)(centreCol + delta), (sbyte)(centreRow - delta)));
                        currentLine.Add(((sbyte)(centreCol - delta), (sbyte)(centreRow + delta)));
                    }
                    Lines.Add(currentLine);

                    //all slices are added.  Now clean-up.
                    foreach(var line in Lines) {
                        line.RemoveAll(pos => !board.IsSpaceInsideOfBoard(pos));
                    }
                    Lines = Lines.Shuffle().ToList();
                }

                // now with our slices ready, we'll also be a bit smarter and abort a slice if it's visibly impossible.
                foreach(var line in Lines) {
                    var lineMarks = line.Select(pos => board.GetSpaceView(pos.Col, pos.Row).Mark);
                    if(lineMarks.All(mark => string.IsNullOrEmpty(mark) || mark.Equals(gameView.Player!.Mark))) {
                        //slice is available to play
                        foreach(var pos in line) {
                            gameView.Attempt(factorySpaceActions[0].Create(board.BoardIndex, pos.Col, pos.Row));
                        }
                    }
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
