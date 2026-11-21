using MnkeyFog.Model.Template;
using MnkeyFog.Model.Views;

namespace MnkeyFog.Model.PlayerAIs {

    /// <summary>
    /// Aster is a human-fixed version of Clod.
    /// </summary>
    [ModelSerializable]
    public class AsterAI : IPlayerAI {
        private readonly Random _random;

        public AsterAI() : this(new Random()) { }

        public AsterAI(int seed) : this(new Random(seed)) { }

        public AsterAI(Random random) {
            _random = random;
        }

        public string Description => "Aster, Difficulty 3";

        // persist these across executions so we'll keep our order of plan from
        // one turn to next.
        List<BoardView>? Boards { get; set; } = null;
        List<List<(sbyte Col, sbyte Row)>>? Lines { get; set; } = null;

        public void Attempt(GameView gameView) {
            var actionFactories = gameView.AvailableActions;
            var factorySpaceActions = new List<GameActionFactoryForSpace>(actionFactories.OfType<GameActionFactoryForSpace>());
            var simpleFactory = actionFactories.OfType<GameActionFactoryForSimple>().FirstOrDefault();

            //shuffle the boards so it won't be a consistent pattern of where it starts.
            if (Boards == null) {
                Boards = SeededShuffle(gameView.Boards, _random);
            }

            // Iterate ALL boards - handles multi-board games correctly
            foreach (var board in Boards) {

                // Calculate center: (count - 1) / 2, explicit casts throughout               
                double centreX = (board.ColumnCount - 1) / 2.0;      // force double arithmetic
                double centreY = (board.RowCount - 1) / 2.0;

                var centreCol = centreX.FloorAsSByte;
                var centreRow = centreY.FloorAsSByte;

                if (Lines == null) {
                    Lines = new List<List<(sbyte Col, sbyte Row)>>();
                    var currentLine = new List<(sbyte Col, sbyte Row)>();

                    // Vertical scan through center column
                    currentLine.Clear();
                    currentLine.Add((centreCol, centreRow));
                    for (sbyte delta = 1; true; delta += 1) {
                        if ((centreRow + delta >= board.RowCount) && (centreRow - delta < 0)) {
                            break;
                        }
                        currentLine.Add((centreCol, (centreRow + delta).AsSByte));
                        currentLine.Add((centreCol, (centreRow - delta).AsSByte));
                    }
                    Lines.Add(currentLine);

                    // Horizontal scan through center row
                    currentLine.Clear();
                    currentLine.Add((centreCol, centreRow));
                    for (sbyte delta = 1; true; delta += 1) {
                        if ((centreCol + delta >= board.ColumnCount) && (centreCol - delta < 0)) {
                            break;
                        }
                        currentLine.Add(((centreCol + delta).AsSByte, centreRow));
                        currentLine.Add(((centreCol - delta).AsSByte, centreRow));
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
                        currentLine.Add(((centreCol + delta).AsSByte, (centreRow + delta).AsSByte));
                        currentLine.Add(((centreCol - delta).AsSByte, (centreRow - delta).AsSByte));
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
                        currentLine.Add(((centreCol + delta).AsSByte, (centreRow - delta).AsSByte));
                        currentLine.Add(((centreCol - delta).AsSByte, (centreRow + delta).AsSByte));
                    }
                    Lines.Add(currentLine);

                    //all slices are added.  Now clean-up.
                    foreach (var line in Lines) {
                        line.RemoveAll(pos => !board.IsSpaceInsideOfBoard(pos));
                    }
                    Lines = SeededShuffle(Lines, _random);
                }

                // now with our slices ready, we'll also be a bit smarter and abort a slice if it's visibly impossible.
                foreach (var line in Lines) {
                    var lineMarkIndices = line.Select(pos => board.GetSpaceView(pos.Col, pos.Row).MarkIndex);
                    if (lineMarkIndices.All(lineMarkIndex => lineMarkIndex == null || lineMarkIndex != gameView.PlayerIndex)) {
                        //slice is available to play
                        foreach (var pos in line) {
                            gameView.Attempt(factorySpaceActions[0].Create(board.BoardIndex, pos.Col, pos.Row));
                        }
                    }
                }
            }

            // SpaceViews fallback: try each space for coordinate lookup
            foreach (var spaceView in gameView.SpaceViews) {
                if (factorySpaceActions.Count > 0) {
                    gameView.Attempt(factorySpaceActions[0].Create(spaceView.BoardIndex, spaceView.Col, spaceView.Row));
                }
            }

            // Final fallback: simple action
            if (simpleFactory != null) {
                gameView.Attempt(simpleFactory.Create());
            }
        }

        /// <summary>
        /// Fisher-Yates shuffle using the supplied Random, so a seeded AsterAI is
        /// deterministic.
        /// </summary>
        private static List<T> SeededShuffle<T>(IEnumerable<T> source, Random random) {
            var list = source.ToList();
            for (var i = list.Count - 1; i > 0; i--) {
                var j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }
    }
}
