namespace MnkeyFog.Model.Tests;

public class BoardTests {
    #region board size
    [Fact]
    public void Width_Default() {
        var board = new Board() {};
        board.ColumnCount.Should().Be(1);
    }

    [Fact]
    public void Width_3x3() {
        var board = new Board(3, 3, new MNKBoardRuleset());
        board.ColumnCount.Should().Be(3);
    }

    [Fact]
    public void Width_26() {
        var board = new Board(26, 10, new MNKBoardRuleset());
        board.ColumnCount.Should().Be(26);
    }

    [Fact]
    public void Height_Default() {
        var board = new Board();
        board.RowCount.Should().Be(1);
    }

    [Fact]
    public void Height_3x3() {
        var board = new Board(3, 3, new MNKBoardRuleset());
        board.RowCount.Should().Be(3);
    }

    [Fact]
    public void Height_10() {
        var board = new Board(26, 10, new MNKBoardRuleset());
        board.RowCount.Should().Be(10);
    }
    #endregion

    #region GetBoardAsEnumerable
    [Fact]
    public void BoardAsEnumerable_ReturnsAllSpaces_ExpectedCount() {
        var board = new Board(3, 3, new MNKBoardRuleset());
        var spaces = board.AsSpaceEnumerable().ToList();
        spaces.Count.Should().Be(9);
    }

    [Fact]
    public void BoardAsEnumerable_ReturnsAllSpaces_26x10() {
        var board = new Board(26, 10, new MNKBoardRuleset());
        var spaces = board.AsSpaceEnumerable().ToList();
        spaces.Count.Should().Be(260);
    }
    #endregion

    #region MakeKnownToPlayer

    [Fact]
    public void MakeKnownToPlayer_MarksToPlayer_IsKnown() {
        var board = new Board(3, 3, new MNKBoardRuleset());
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[0, 0].MakeKnownToPlayer("X");

        board.Spaces[0, 0].KnownToPlayersSet.Should().Contain("X");
    }

    [Fact]
    public void MakeKnownToPlayer_MarksToAnotherPlayer_IsKnown() {
        var board = new Board(3, 3, new MNKBoardRuleset());
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[0, 0].MakeKnownToPlayer("O");

        board.Spaces[0, 0].KnownToPlayersSet.Should().Contain("O");
    }
    #endregion
}
