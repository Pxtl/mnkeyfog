namespace KriegspielTicTacToe.Model;


/// <summary>
/// JSON-serializable model object for a single tic-tac-toe board.
/// </summary>
public record Board {
    #region constructors
    public Board() {}
    public Board(BoardBuilder builder) : this(builder.Width, builder.Height, builder.ScoringLength) {}
    public Board(sbyte width, sbyte height) : this(width, height, null) {}
    public Board(sbyte width, sbyte height, sbyte? scoringLength = null, bool isBoardDoneWhenScored = false) {
        Spaces = new Space[width, height];
        for (var col = 0; col < Spaces.GetLength(0); col+=1) {
            for (var row = 0; row < Spaces.GetLength(1); row+=1) {
                Spaces[col,row] = new Space();
            }
        }

        if (scoringLength.HasValue) {
            HorizontalScoringLength = scoringLength.Value;
            VerticalScoringLength = scoringLength.Value;
            DiagonalScoringLength = scoringLength.Value;
        } else {
            HorizontalScoringLength = width;
            VerticalScoringLength = height;
            DiagonalScoringLength = Math.Min(width, height);
        }

        IsBoardDoneWhenScored = isBoardDoneWhenScored;
    }
    #endregion

    #region main data properties
    public Space[,] Spaces {get;set;} = new Space[1,1]{{new Space()}}; //default value is dummy board, never use.
    public sbyte HorizontalScoringLength {get;init;}
    public sbyte VerticalScoringLength {get;init;}
    public sbyte DiagonalScoringLength {get;init;}
    public bool IsBoardDoneWhenScored {get;init;}
    #endregion

    #region Methods
    /// <summary>
    /// For the given space on the board, generate the space's name.
    /// </summary>
    public int GetSpaceNameAsInt(int col, int row) {
        //aims for basic 3x3, but larger if needed
        //7 8 9
        //4 5 6
        //1 2 3
        return Spaces.GetLength(1) * (Spaces.GetLength(0) - 1) //top-left
            + col
            - row * Spaces.GetLength(0)
            + 1; //1-based
    }

    /// <summary>
    /// For the given space index code, find the coordinates.  Uses a "Try"
    /// signature so that it shall return false if the given spaceindex is not
    /// on the board at all.
    /// </summary>
    public bool TryGetCoordinatesFromSpaceNameAsInt(int spaceName, out int resultCol, out int resultRow) {
        //brute-force search
        //todo: smarter algo
        for (var col = 0; col < Spaces.GetLength(0); col+=1) {
            for (var row = 0; row < Spaces.GetLength(1); row+=1) {
                if(GetSpaceNameAsInt(col, row) == spaceName) {
                    resultCol = col;
                    resultRow = row;
                    return true;
                }
            }
        }
        resultCol = resultRow = -1;
        return false;
    }
        
    /// <summary>
    /// Get all of the spaces on the board as a big enumerable that you can
    /// foreach across.
    /// </summary>
    public IEnumerable<Space> BoardAsEnumerable() {
        foreach(var space in Spaces) {
            yield return space;    
        }
    }
    #endregion

    #region helper properties
    [JsonIgnore()]
    public int ColumnCount
        => Spaces.GetLength(0);

    [JsonIgnore()]    
    public int RowCount
        => Spaces.GetLength(1);
        
    /// <summary>
    /// Get how many spaces are on the board.
    /// </summary>
    [JsonIgnore()]
    public int SpaceCount
        => Spaces.GetLength(0) * Spaces.GetLength(1);

    /// <summary>
    /// Get how many digits the users will have to type in to type in a
    /// space-name.
    /// </summary>
    [JsonIgnore()]
    public int SpaceNameLength
        => SpaceCount.ToString().Length;

    /// <summary>
    /// Returns true if the game has ended in a tie.
    /// </summary>
    [JsonIgnore()]
    public bool IsFull 
        => BoardAsEnumerable().All(s => s.Mark != null);

    [JsonIgnore()]
    public bool IsDone
        => IsFull 
        || (IsBoardDoneWhenScored && ScoreCard.PlayerScores.Any(s => s.Score > 0));

    [JsonIgnore()]
    public ScoreCard ScoreCard {
        get {
            var result = new ScoreCard();
            var width = Spaces.GetLength(0);
            var height = Spaces.GetLength(1);
            
            for(sbyte row = 0; row < height; row+=1) {
                for(sbyte col = 0; col < width; col+=1) {
                    string? lineOwnerMark = Spaces[col,row].Mark;
                    if(lineOwnerMark != null) {
                        var lineOwnerPlayer = new Player(lineOwnerMark);
                        result += ScoreSpace(lineOwnerPlayer, (col, row));
                    }
                }
            }
            return result;
        }
    }

    public ScoreCard ScoreSpace(
        Player lineOwnerPlayer,
        (sbyte Col, sbyte Row) pos
    ) => ScoreSpace(lineOwnerPlayer, pos, (1, 0), HorizontalScoringLength)
        + ScoreSpace(lineOwnerPlayer, pos, (0, 1), VerticalScoringLength)
        + ScoreSpace(lineOwnerPlayer, pos, (1, 1), DiagonalScoringLength)
        + ScoreSpace(lineOwnerPlayer, pos, (1, -1), DiagonalScoringLength);

    /// <summary>
    /// Score a given space for the given player and the given direction. Only
    /// counts score for lines that *start* on the space, not ones that continue
    /// on the space.
    /// </summary>
    /// <param name="lineOwnerPlayer"></param>
    /// <param name="lineStartPos"></param>
    /// <param name="delta"></param>
    /// <param name="scoreLen"></param>
    /// <returns></returns>
    public ScoreCard ScoreSpace(
        Player lineOwnerPlayer,
        (sbyte Col, sbyte Row) lineStartPos,
        (sbyte Col, sbyte Row) delta,
        int scoreLen
    ) {
        var boardSize = ((sbyte)Spaces.GetLength(0), (sbyte)Spaces.GetLength(1));
        (sbyte Col, sbyte Row) endPos = ExtrapolatePos(lineStartPos, delta, scoreLen - 1);

        //end point is outside of board.
        if (!IsSpaceInsideOfBoard(endPos, boardSize)) {
            return ScoreCard.Empty;
        }

        (sbyte Col, sbyte Row) beforeStartPos = ExtrapolatePos(lineStartPos, delta, -1);
        if (
            IsSpaceInsideOfBoard(beforeStartPos, boardSize)
            && Spaces[beforeStartPos.Col, beforeStartPos.Row].Mark == lineOwnerPlayer.Mark
        ) {
            // line already started before this space, return false to prevent double-counting.
            return ScoreCard.Empty;
        }

        var lineLength = 0;
        for (sbyte i = 0; IsSpaceInsideOfBoard(ExtrapolatePos(lineStartPos, delta, i), boardSize); i += 1) {
            (sbyte Col, sbyte Row) curPos = ExtrapolatePos(lineStartPos, delta, i);

            if (lineOwnerPlayer.Mark != Spaces[curPos.Col, curPos.Row].Mark) {
                break;
            } else {
                lineLength = i+1;
            }
        }
        var lineScore = lineLength / scoreLen;
        return (lineScore > 0) 
            ? new ScoreCard(lineOwnerPlayer, lineScore)
            : ScoreCard.Empty;
    }

    private static (sbyte Col, sbyte Row) ExtrapolatePos((sbyte Col, sbyte Row) pos, (sbyte Col, sbyte Row) delta, int multiplier)
        => ((sbyte)(pos.Col + delta.Col * multiplier), (sbyte)(pos.Row + delta.Row * multiplier));

    private static bool IsSpaceInsideOfBoard((sbyte Col, sbyte Row) pos, (sbyte Col, sbyte Row) boardSize)
        => (pos.Col < boardSize.Col)
            && (pos.Row < boardSize.Row)
            && (pos.Col >= 0)
            && (pos.Row >= 0);
    #endregion
}
