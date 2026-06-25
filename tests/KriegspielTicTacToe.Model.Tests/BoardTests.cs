namespace KriegspielTicTacToe.Model.Tests;

public class BoardTests {
    #region board size
    [Fact]
    public void Width_Default() {
        var board = new Board() {};
        board.ColumnCount.Should().Be(1);
    }

    [Fact]
    public void Width_3x3() {
        var board = new Board(3, 3, new MNKRuleset());
        board.ColumnCount.Should().Be(3);
    }

    [Fact]
    public void Width_100() {
        var board = new Board(100, 10, new MNKRuleset());
        board.ColumnCount.Should().Be(100);
    }

    [Fact]
    public void Height_Default() {
        var board = new Board();
        board.RowCount.Should().Be(1);
    }

    [Fact]
    public void Height_3x3() {
        var board = new Board(3, 3, new MNKRuleset());
        board.RowCount.Should().Be(3);
    }

    [Fact]
    public void Height_10() {
        var board = new Board(100, 10, new MNKRuleset());
        board.RowCount.Should().Be(10);
    }
    #endregion

    #region SpaceNameLength
    [Fact]
    public void SpaceNameLength_3x3Board_Returns1() {
        var board = new Board(3, 3, new MNKRuleset());
        board.SpaceNameLength.Should().Be(1);
    }

    [Fact]
    public void SpaceNameLength_100x10Board_ReturnsCorrect() {
        var board = new Board(100, 10, new MNKRuleset());
        var spaceCount = board.SpaceCount;
        board.SpaceNameLength.Should().Be((int)Math.Floor(Math.Log10(spaceCount)) + 1);
    }
    #endregion

    #region GetBoardAsEnumerable
    [Fact]
    public void BoardAsEnumerable_ReturnsAllSpaces_ExpectedCount() {
        var board = new Board(3, 3, new MNKRuleset());
        var spaces = board.BoardAsSpaceViewEnumerable().ToList();
        spaces.Count.Should().Be(9);
    }

    [Fact]
    public void BoardAsEnumerable_ReturnsAllSpaces_100x10() {
        var board = new Board(100, 10, new MNKRuleset());
        var spaces = board.BoardAsSpaceViewEnumerable().ToList();
        spaces.Count.Should().Be(1000);
    }
    #endregion

    #region GetSpaceName
    [Fact]
    public void GetSpaceName_3x3TopRightCorner_ReturnsExpected() {
        // top-right corner (row 0, col 2) in 3x3
        // board matches layout of numpad, 1 is bottom left.
        var board = new Board(3, 3, new MNKRuleset());
        var code = board.GetSpaceName(2, 0);
        code.Should().Be("9");
    }

    [Fact]
    public void GetSpaceName_4x4LeftLowerMiddleSquare_ReturnsExpected() {
        // 4x4 board: (row 2, col 0)
        // 01 is bottom left, goes right-then-up.
        // because this is more than 3x3 it will > 9 spaces so 2 digit.
        var board = new Board(4, 4, new MNKRuleset());
        var code = board.GetSpaceName(0, 2);
        code.Should().Be("05");
    }
    #endregion

    #region TryGetCoordinatesFromSpaceName
    [Fact]
    public void TryGetCoordinatesFromSpaceName_Valid() {
        var board = new Board(3, 3, new MNKRuleset());
        var ok = board.TryGetCoordinatesFromSpaceNameAsInt(1, out var col, out var row);
        ok.Should().BeTrue();
        col.Should().BeLessThan(board.ColumnCount);
        row.Should().BeLessThan(board.RowCount);
    }

    [Fact]
    public void TryGetCoordinatesFromSpaceName_Invalid() {
        var board = new Board(3, 3, new MNKRuleset());
        var ok = board.TryGetCoordinatesFromSpaceNameAsInt(99, out _, out _);
        ok.Should().BeFalse();
    }
    #endregion

    #region MakeKnownToPlayer

    [Fact]
    public void MakeKnownToPlayer_MarksToPlayer_IsKnown() {
        var board = new Board(3, 3, new MNKRuleset());
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[0, 0].MakeKnownToPlayer("X");

        board.Spaces[0, 0].KnownToPlayersSet.Should().Contain("X");
    }

    [Fact]
    public void MakeKnownToPlayer_MarksToAnotherPlayer_IsKnown() {
        var board = new Board(3, 3, new MNKRuleset());
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[0, 0].MakeKnownToPlayer("O");

        board.Spaces[0, 0].KnownToPlayersSet.Should().Contain("O");
    }
    #endregion
}
