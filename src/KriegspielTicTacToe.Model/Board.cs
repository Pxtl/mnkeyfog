using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using KriegspielTicTacToe.Model.Views;
using OneOf;

namespace KriegspielTicTacToe.Model;

/// <summary>
/// JSON-serializable model object for a single tic-tac-toe board. Columns are
/// left-to-right, rows are top-to-bottom.
/// </summary>
[ModelSerializable]
public sealed record Board {
    #region constructors
    /// <summary>
    /// Default constructor creates a useless board.  Never uses this without
    /// replacing <see cref="Ruleset"> and <see cref="Spaces"/> members.
    /// </summary>
    public Board()
    : this(1, 1, BoardRuleset.Empty) { }

    public Board(Template.BoardBuilder builder)
    : this(builder.Width, builder.Height, builder.Ruleset) { }

    public Board(sbyte width, sbyte height)
    : this(width, height, null) { }

    public Board(sbyte width, sbyte height, BoardRuleset? ruleset) {
        Ruleset = ruleset ?? BoardRuleset.Empty;
        Spaces = new Space[width, height];
        for (sbyte col = 0; col < ColumnCount; col += 1) {
            for (sbyte row = 0; row < RowCount; row += 1) {
                Spaces[col, row] = new Space();
            }
        }
    }

    public Board(BoardRuleset ruleset)
    : this() {
        Ruleset = ruleset;
    }
    #endregion

    #region main data properties
    [Required]
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.None, TypeNameHandling = TypeNameHandling.None)] //non-polymorphic
    public Space[,] Spaces { get; init; }
    [Required]
    public BoardRuleset Ruleset { get; init; }
    #endregion

    #region Methods
    //TODO: AttemptSpace command needed... maybe 

    /// <summary>
    /// Get all of the spaces on the board as a big enumerable that you can
    /// foreach across.
    /// </summary>
    public IEnumerable<SpaceView> AsSpaceViewEnumerable(Player? player) {
        for (sbyte col = 0; col < Spaces.GetLength(0); col += 1) {
            for (sbyte row = 0; row < Spaces.GetLength(1); row += 1) {
                yield return new SpaceView(Spaces[col, row], player, col, row);
            }
        }
    }

    public IEnumerable<Space> AsSpaceEnumerable() {
        for (sbyte col = 0; col < Spaces.GetLength(0); col += 1) {
            for (sbyte row = 0; row < Spaces.GetLength(1); row += 1) {
                yield return Spaces[col, row];
            }
        }
    }

    public IEnumerable<SpaceView> AsSpaceViewEnumerable()
    => AsSpaceViewEnumerable(player: null);
    #endregion

    #region abstract and virtual properties
    [JsonIgnore()]
    public ScoreCard ScoreCard => Ruleset.Score(this);
    #endregion

    #region helper properties
    [JsonIgnore()]
    public sbyte ColumnCount
    => (sbyte)Spaces.GetLength(0);

    [JsonIgnore()]
    public sbyte RowCount
    => (sbyte)Spaces.GetLength(1);

    /// <summary>
    /// Get how many spaces are on the board.
    /// </summary>
    [JsonIgnore()]
    public int SpaceCount
    => Spaces.GetLength(0) * Spaces.GetLength(1);

    /// <summary>
    /// Returns true if the board is full.
    /// </summary>
    [JsonIgnore()]
    public bool IsFull
    => AsSpaceEnumerable().All(s => s.Mark != null);

    /// <summary>
    /// Returns true if the board is done and locked from further play.
    /// </summary>
    [JsonIgnore()]
    public bool IsDone
    => IsFull || Ruleset.IsDone(this);
    #endregion

    /// <summary>
    /// Returns true if a space is within an arbitrarily-sized board.
    /// </summary>
    public bool IsSpaceInsideOfBoard((sbyte Col, sbyte Row) pos, (sbyte Col, sbyte Row) boardSize)
    => (pos.Col < boardSize.Col)
        && (pos.Row < boardSize.Row)
        && (pos.Col >= 0)
        && (pos.Row >= 0);
}
