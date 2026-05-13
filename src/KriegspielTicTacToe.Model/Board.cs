namespace KriegspielTicTacToe.Model;

using System.Linq;
using System.IO;
using Newtonsoft.Json;
using OneOf;

/// <summary>
/// JSON-serializable model object for a single tic-tac-toe board.
/// </summary>
public record Board {
    #region constructors
    public Board() {}
    public Board(BoardBuilder builder) : this(builder.Width, builder.Height) {}
    public Board(byte width, byte height) {
        Spaces = new Space[width, height];
        for (var col = 0; col < Spaces.GetLength(0); col+=1) {
            for (var row = 0; row < Spaces.GetLength(1); row+=1) {
                Spaces[col,row] = new Space();
            }
        }
    }
    #endregion

    #region main data properties
    public Space[,] Spaces {get;set;} = new Space[1,1]{{new Space()}}; //default value is dummy board, never use.
    #endregion

    #region Methods
    /// <summary>
    /// For the given space on the board, generate the space's index code.
    /// </summary>
    public int GetSpaceIndexCode(int col, int row) {
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
    public bool TryGetCoordinatesFromSpaceIndexCode(int spaceIndex, out int resultCol, out int resultRow) {
        //brute-force search
        //todo: smarter algo
        for (var col = 0; col < Spaces.GetLength(0); col+=1) {
            for (var row = 0; row < Spaces.GetLength(1); row+=1) {
                if(GetSpaceIndexCode(col, row) == spaceIndex) {
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
    public System.Collections.Generic.IEnumerable<Space> BoardAsEnumerable() {
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
    /// space-code.
    /// </summary>
    [JsonIgnore()]
    public int SpaceIndexCodeLength
        => SpaceCount.ToString().Length;

    /// <summary>
    /// Returns true if the game has ended in a tie.
    /// </summary>
    [JsonIgnore()]
    public bool IsFull 
        => BoardAsEnumerable().All(s => s.MarkChar.HasValue);

    [JsonIgnore()]
    public bool IsDone
        => IsFull 
        || ScoreCard.HighestScore.HasValue;

    [JsonIgnore()]
    public ScoreCard ScoreCard {
        get {
            var result = new System.Collections.Generic.List<PlayerScore>();
            //search for full-rows
            for(int row = 0; row < Spaces.GetLength(1); row+=1) {
                bool isWinner = true;
                var comparator = Spaces[0,row].MarkChar;
                if(comparator.HasValue) {
                    for(int col = 1; col < Spaces.GetLength(0); col+=1) {
                        if(comparator != Spaces[col,row].MarkChar) {
                            isWinner = false;
                            break;
                        }
                    }
                    if(isWinner) {
                        result.Add(new PlayerScore(comparator.Value, 1));
                    }
                }
            }
            //search for full-columns
            //todo: deduplicate.  Rotate array and re-run?
            for(int col = 0; col < Spaces.GetLength(0); col+=1) {
                bool isWinner = true;
                var comparator = Spaces[col,0].MarkChar;
                if(comparator.HasValue) {
                    for(int row = 1; row < Spaces.GetLength(1); row+=1) {
                        if(comparator != Spaces[col,row].MarkChar) {
                            isWinner = false;
                            break;
                        }
                    }
                    if(isWinner) {
                        result.Add(new PlayerScore(comparator.Value, 1));
                    }
                }
            }
            
            //create dummy scope to hide comparator var
            {
                //todo: support non-square Spacess, deduplicate.
                //identity diagonal
                var comparator = Spaces[0,0].MarkChar;
                if(comparator.HasValue) {
                    bool isWinner = true;
                    for(int col=1; col < Spaces.GetLength(0) && col < Spaces.GetLength(1); col+=1) {
                        var row = col;
                        if(comparator != Spaces[col,row].MarkChar) {
                            isWinner = false;
                            break;
                        }
                    }
                    if(isWinner) {
                        result.Add(new PlayerScore(comparator.Value, 1));
                    }
                }
            }
            //create dummy scope to hide comparator var
            {
                //inverse diagonal
                var comparator = Spaces[0,Spaces.GetLength(1)-1].MarkChar;
                if(comparator.HasValue) {
                    bool isWinner = true;
                    for(int col=1; col < Spaces.GetLength(0) && col < Spaces.GetLength(1); col+=1) {
                        var row = Spaces.GetLength(1) - 1 - col;
                        if(comparator != Spaces[col,row].MarkChar) {
                            isWinner = false;
                            break;
                        }
                    }
                    if(isWinner) {
                        result.Add(new PlayerScore(comparator.Value, 1));
                    }
                }
            }
            return new ScoreCard(result);
        }
    }
    #endregion
}
