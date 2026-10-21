using Newtonsoft.Json;

namespace MnkeyFog.Text;

public record class BoardRendererConfiguration {
    #region constructors
    public BoardRendererConfiguration(
        int cellWidth,
        char? cellColumnSeparator, char? cellRowSeparator,
        char? topLeftCorner, char? topIntersection, char? topRightCorner,
        char? leftIntersection, char? middleIntersection, char? rightIntersection,
        char? bottomLeftCorner, char? bottomIntersection, char? bottomRightCorner,
        string emptyBoardLeader, string doneBoardLeader, Func<string, string> boardNameMappingToBoardLeader
    ) {
        CellWidth = cellWidth;
        CellColumnSeparatorChar = cellColumnSeparator;
        CellRowSeparatorChar = cellRowSeparator;
        TopLeftCornerChar = topLeftCorner;
        TopIntersectionChar = topIntersection;
        TopRightCornerChar = topRightCorner;
        LeftIntersectionChar = leftIntersection;
        MiddleIntersectionChar = middleIntersection;
        RightIntersectionChar = rightIntersection;
        BottomLeftCornerChar = bottomLeftCorner;
        BottomIntersectionChar = bottomIntersection;
        BottomRightCornerChar = bottomRightCorner;
        EmptyBoardLeader = emptyBoardLeader;
        DoneBoardLeader = doneBoardLeader;
        BoardNameMappingToBoardLeader = boardNameMappingToBoardLeader;
    }
    #endregion

    #region Data members
    public int CellWidth { get; }
    public char? CellColumnSeparatorChar { get; }
    public char? CellRowSeparatorChar { get; }
    public char? TopLeftCornerChar { get; }
    public char? TopIntersectionChar { get; }
    public char? TopRightCornerChar { get; }
    public char? LeftIntersectionChar { get; }
    public char? MiddleIntersectionChar { get; }
    public char? RightIntersectionChar { get; }
    public char? BottomLeftCornerChar { get; }
    public char? BottomIntersectionChar { get; }
    public char? BottomRightCornerChar { get; }
    public string EmptyBoardLeader { get; }
    public string DoneBoardLeader { get; }
    public Func<string, string> BoardNameMappingToBoardLeader { get; }
    #endregion

    private readonly string FinalFailoverString = " ";

    #region Calculated members

    [JsonIgnore]
    public string TopLeftCorner {
        get {
            if (HasLeftBorder && HasTopBorder) {
                return TopLeftCornerChar?.ToString() ?? FinalFailoverString;
            }
            return "";
        }
    }

    [JsonIgnore]
    public string TopIntersection {
        get {
            if (HasTopBorder) {
                if (HasColumnSeparators) {
                    return TopIntersectionChar?.ToString() ?? FinalFailoverString;
                } else {
                    return CellRowSeparatorChar?.ToString() ?? FinalFailoverString;
                }
            }
            return "";
        }
    }

    [JsonIgnore]
    public string TopRightCorner {
        get {
            if (HasRightBorder && HasTopBorder) {
                return TopRightCornerChar?.ToString() ?? FinalFailoverString;
            }
            return "";
        }
    }

    [JsonIgnore]
    public string LeftIntersection {
        get {
            if (HasLeftBorder) {
                if (HasRowSeparators) {
                    return LeftIntersectionChar?.ToString() ?? FinalFailoverString;
                } else {
                    return CellColumnSeparatorChar?.ToString() ?? FinalFailoverString;
                }
            }
            return "";
        }
    }

    [JsonIgnore]
    public string MiddleIntersection {
        get {
            if (HasColumnSeparators && HasRowSeparators) {
                return MiddleIntersectionChar?.ToString() ?? FinalFailoverString;
            } else if (HasRowSeparators) {
                return CellRowSeparatorChar?.ToString() ?? FinalFailoverString;
            } else if (HasColumnSeparators) {
                return CellColumnSeparatorChar?.ToString() ?? FinalFailoverString;
            }
            return "";
        }
    }

    [JsonIgnore]
    public string RightIntersection {
        get {
            if (HasRightBorder) {
                if (HasRowSeparators) {
                    return RightIntersectionChar?.ToString() ?? FinalFailoverString;
                } else {
                    return CellColumnSeparatorChar?.ToString() ?? FinalFailoverString;
                }
            }
            return "";
        }
    }


    [JsonIgnore]
    public string BottomLeftCorner {
        get {
            if (HasLeftBorder && HasBottomBorder) {
                return BottomLeftCornerChar?.ToString() ?? FinalFailoverString;
            }
            return "";
        }
    }

    [JsonIgnore]
    public string BottomIntersection {
        get {
            if (HasBottomBorder) {
                if (HasColumnSeparators) {
                    return BottomIntersectionChar?.ToString() ?? FinalFailoverString;
                } else {
                    return CellRowSeparatorChar?.ToString() ?? FinalFailoverString;
                }
            }
            return "";
        }
    }

    [JsonIgnore]
    public string BottomRightCorner {
        get {
            if (HasRightBorder && HasBottomBorder) {
                return BottomRightCornerChar?.ToString() ?? FinalFailoverString;
            }
            return "";
        }
    }

    [JsonIgnore]
    public string CellRowSeparator
    => HasRowSeparators
        ? new string(CellRowSeparatorChar ?? FinalFailoverString[0], CellWidth)
        : "";

    [JsonIgnore]
    public string CellColumnSeparator
    => HasColumnSeparators
        ? (CellColumnSeparatorChar.ToString() ?? FinalFailoverString)
        : "";

    [JsonIgnore]
    public bool HasTopBorder => TopLeftCornerChar != null || TopRightCornerChar != null;

    [JsonIgnore]
    public bool HasBottomBorder => BottomLeftCornerChar != null || BottomRightCornerChar != null;

    [JsonIgnore]
    public bool HasLeftBorder => TopLeftCornerChar != null || BottomLeftCornerChar != null;

    [JsonIgnore]
    public bool HasRightBorder => TopRightCornerChar != null || BottomRightCornerChar != null;

    [JsonIgnore]
    public bool HasColumnSeparators => TopIntersectionChar != null || MiddleIntersectionChar != null || BottomIntersectionChar != null;

    [JsonIgnore]
    public bool HasRowSeparators => LeftIntersectionChar != null || MiddleIntersectionChar != null || RightIntersectionChar != null;
    #endregion

    // Factory methods for static configurations
    public static BoardRendererConfiguration FullPipes { get; } = new BoardRendererConfiguration(
        3, '│', '─',
        '┌', '┬', '┐',
        '├', '┼', '┤',
        '└', '┴', '┘',
        "  ", " ✓", boardName => boardName.PadLeft(2)
    );

    public static BoardRendererConfiguration Borderless { get; } = new BoardRendererConfiguration(
        3, null, null,
        null, null, null,
        null, null, null,
        null, null, null,
        "  ", " ✓", boardName => boardName.PadLeft(2)
    );

    public static BoardRendererConfiguration HashPipes { get; } = new BoardRendererConfiguration(
        3, '│', '─',
        null, null, null,
        null, '┼', null,
        null, null, null,
        "  ", " ✓", boardName => boardName.PadLeft(2)
    );
}
