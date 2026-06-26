namespace KriegspielTicTacToe.Model.Tests;

public class GameViewTests {
    private GameView CreateGameView(params BoardBuilder[] builders) {
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate(builders, isSynchronousMode: false, isKriegspiel: false),
            isRandomPlayerOrder: false
        );        
        return state.GetView(null);
    }
    
    #region GetSpaceName
    [Fact]
    public void GetSpaceName_3x3TopRightSpace_AsExpected() {
        // top-right corner (row 0, col 2) in 3x3
        // board matches layout of numpad, 1 is bottom left.
        var code = CreateGameView(new BoardBuilder(3, 3)).GetSpaceName("", 2, 0);
        code.Should().Be("9");
    }

    [Fact]
    public void GetSpaceName_4x4BottomLeftSpace_AsExpected() {
        // 4x4 board: (row 3, col 0), uses letter-number format, bottom left space
        var code = CreateGameView(new BoardBuilder(4, 4)).GetSpaceName("", 0, 3);
        code.Should().Be("A1");
    }
    #endregion

    #region TryGetCoordinatesFromSpaceName
    [Fact]
    public void TryGetCoordinatesFrom3x3SpaceName_AsExpected() {
        var ok = CreateGameView(new BoardBuilder(3, 3, new MNKBoardRuleset()))
            .TryGetCoordinatesFromSpaceName("1", out string boardName, out var col, out var row);
        ok.Should().BeTrue();
        boardName.Should().Be("1");
        col.Should().Be(0);
        row.Should().Be(2);
    }

    [Fact]
    public void TryGetCoordinatesFrom3x3SpaceName_Invalid() {
        var ok = CreateGameView(new BoardBuilder(3, 3, new MNKBoardRuleset())).TryGetCoordinatesFromSpaceName("99", out string _, out _, out _);
        ok.Should().BeFalse();
    }

    [Fact]
    public void TryGetCoordinatesFrom4x4BottomLeftSpace_AsExpected() {
        // 4x4 board: (row 3, col 0), uses letter-number format, bottom left space
        var ok = CreateGameView(new BoardBuilder(4, 4, new MNKBoardRuleset())).TryGetCoordinatesFromSpaceName("A1", out string boardName, out var col, out var row);
        boardName.Should().Be("1");
        ok.Should().BeTrue();
        col.Should().Be(0);
        row.Should().Be(3);
    }
    #endregion

    #region SpaceNameLength
    [Fact]
    public void SpaceNameLength_3x3Board_Returns1() {
        var view = CreateGameView(new BoardBuilder(3, 3, new MNKBoardRuleset()));
        var board = view.GetBoardViewByIndex(0);
        view.SpaceNameLength(board).Should().Be(1);
    }

    [Fact]
    public void SpaceNameLength_26x10Board_ReturnsCorrect() {
        var view = CreateGameView(new BoardBuilder(26, 10, new MNKBoardRuleset()));
        var board = view.GetBoardViewByIndex(0);
        var spaceCount = board.SpaceCount;
        view.SpaceNameLength(board).Should().Be(3); //2 digit s for 1-10, 1 digit for letter.
    }
    #endregion
}