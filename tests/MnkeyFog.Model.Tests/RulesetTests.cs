namespace MnkeyFog.Model.Tests;

public class RulesetTests {
    const int _playerXIndex = 0;
    const int _playerOIndex = 1;
    #region orthogonal tests

    [Fact]
    public void Given3x3Board_WhenVerticalFull_ThenXWins() {
        var board = new Board(3, 3, new MNKBoardRuleset());

        board.Spaces[0, 0].MarkIndex = _playerXIndex;
        board.Spaces[0, 1].MarkIndex = _playerXIndex;
        board.Spaces[0, 2].MarkIndex = _playerXIndex;

        var expectedPlayerScore = new PlayerIndexScore(_playerXIndex, 1);
        board.ExecuteRuleset();
        board.ScoreCard.Highest!.Should().Be(expectedPlayerScore);
        board.IsDone.Should().BeFalse();
    }

    [Fact]
    public void Given3x3Board_WhenLineFullAndBoardIsDoneWhenScored_ThenBoardIsDone() {
        var board = new Board(3, 3, new MNKBoardRuleset(IsBoardDoneWhenScored: true));

        board.Spaces[0, 0].MarkIndex = _playerXIndex;
        board.Spaces[0, 1].MarkIndex = _playerXIndex;
        board.Spaces[0, 2].MarkIndex = _playerXIndex;

        var expectedPlayerScore = new PlayerIndexScore(_playerXIndex, 1);
        board.ExecuteRuleset();
        board.ScoreCard.Highest!.Should().Be(expectedPlayerScore);
        board.IsDone.Should().BeTrue();
    }

    [Fact]
    public void Given3x3Board_WhenLineFullAndBoardIsNotDoneWhenScored_ThenBoardIsNotDone() {
        var board = new Board(3, 3, new MNKBoardRuleset(IsBoardDoneWhenScored: false));

        board.Spaces[0, 0].MarkIndex = _playerXIndex;
        board.Spaces[0, 1].MarkIndex = _playerXIndex;
        board.Spaces[0, 2].MarkIndex = _playerXIndex;

        var expectedPlayerScore = new PlayerIndexScore(_playerXIndex, 1);
        board.ExecuteRuleset();
        board.ScoreCard.Highest!.Should().Be(expectedPlayerScore);
        board.IsDone.Should().BeFalse();
    }

    [Fact]
    public void Given3x3Board_WhenHorizontalFullWithMultipleWinningRows_ThenMajorityWins() {
        var board = new Board(3, 3, new MNKBoardRuleset());

        board.Spaces[0, 0].MarkIndex = _playerXIndex;
        board.Spaces[1, 0].MarkIndex = _playerXIndex;
        board.Spaces[2, 0].MarkIndex = _playerXIndex;

        board.Spaces[0, 1].MarkIndex = _playerOIndex;
        board.Spaces[1, 1].MarkIndex = _playerOIndex;
        board.Spaces[2, 1].MarkIndex = _playerOIndex;

        board.Spaces[0, 2].MarkIndex = _playerOIndex;
        board.Spaces[1, 2].MarkIndex = _playerOIndex;
        board.Spaces[2, 2].MarkIndex = _playerOIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerOIndex, 2));
        board.IsDone.Should().BeTrue();
    }
    #endregion
    #region 3x3 square boards - baseline diagonal tests

    [Fact]
    public void Given3x3Board_WhenIdentityDiagonal_ThenXWins() {
        var board = new Board(3, 3, new MNKBoardRuleset());

        board.Spaces[0, 0].MarkIndex = _playerXIndex;
        board.Spaces[1, 1].MarkIndex = _playerXIndex;
        board.Spaces[2, 2].MarkIndex = _playerXIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }

    [Fact]
    public void Given3x3Board_WhenInverseDiagonal_ThenXWins() {
        var board = new Board(3, 3, new MNKBoardRuleset());

        board.Spaces[0, 2].MarkIndex = _playerXIndex;
        board.Spaces[1, 1].MarkIndex = _playerXIndex;
        board.Spaces[2, 0].MarkIndex = _playerXIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }
    #endregion

    #region 4x3 rectangular boards (W=4, H=3, diagLen=3) - diagonal tests

    [Fact]
    public void Given4x3Board_WhenIdentityDiagonal_ThenXWins() {
        var board = new Board(4, 3, new MNKBoardRuleset());

        // Identity diagonal: starts at (0, H-diagLen) = (0, 0), ends at (diagLen-1, H-1) = (2, 2)
        board.Spaces[0, 0].MarkIndex = _playerXIndex;
        board.Spaces[1, 1].MarkIndex = _playerXIndex;
        board.Spaces[2, 2].MarkIndex = _playerXIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }

    [Fact]
    public void Given4x3Board_WhenIdentityDiagonalOffset_ThenXWins() {
        var board = new Board(4, 3, new MNKBoardRuleset());

        // Identity diagonal: starts at (0, H-diagLen) = (0, 0), ends at (diagLen-1, H-1) = (2, 2)
        board.Spaces[1, 0].MarkIndex = _playerXIndex;
        board.Spaces[2, 1].MarkIndex = _playerXIndex;
        board.Spaces[3, 2].MarkIndex = _playerXIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }

    [Fact]
    public void Given4x3Board_WhenInverseDiagonal_ThenXWins() {
        var board = new Board(4, 3, new MNKBoardRuleset());

        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[0, 2].MarkIndex = _playerXIndex;
        board.Spaces[1, 1].MarkIndex = _playerXIndex;
        board.Spaces[2, 0].MarkIndex = _playerXIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }

    [Fact]
    public void Given4x3Board_WhenInverseDiagonalOffset_ThenXWins() {
        var board = new Board(4, 3, new MNKBoardRuleset());

        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[1, 2].MarkIndex = _playerXIndex;
        board.Spaces[2, 1].MarkIndex = _playerXIndex;
        board.Spaces[3, 0].MarkIndex = _playerXIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }

    [Fact]
    public void Given4x3Board_WhenDiagonalEdgeToEdgeOnly2InLine_ThenNoScore() {
        var board = new Board(4, 3, new MNKBoardRuleset(IsBoardDoneWhenScored: true));

        // Only 2 X's inline on diagonal - not a winning line
        board.Spaces[0, 1].MarkIndex = _playerXIndex;
        board.Spaces[1, 2].MarkIndex = _playerXIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.Should().Be(ScoreCard.Empty);
        board.IsDone.Should().BeFalse();
    }
    #endregion

    #region 3x4 rectangular boards (W=3, H=4, diagLen=3) - diagonal tests

    [Fact]
    public void Given3x4Board_WhenIdentityDiagonal_ThenXWins() {
        var board = new Board(3, 4, new MNKBoardRuleset());

        // Identity diagonal: starts at (0, H-diagLen) = (0, 1), ends at (diagLen-1, H-1) = (2, 3)
        board.Spaces[0, 0].MarkIndex = _playerXIndex;
        board.Spaces[1, 1].MarkIndex = _playerXIndex;
        board.Spaces[2, 2].MarkIndex = _playerXIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }

    [Fact]
    public void Given3x4Board_WhenIdentityDiagonalOffset_ThenXWins() {
        var board = new Board(3, 4, new MNKBoardRuleset());

        // Identity diagonal: starts at (0, H-diagLen) = (0, 1), ends at (diagLen-1, H-1) = (2, 3)
        board.Spaces[0, 1].MarkIndex = _playerXIndex;
        board.Spaces[1, 2].MarkIndex = _playerXIndex;
        board.Spaces[2, 3].MarkIndex = _playerXIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }

    [Fact]
    public void Given3x4Board_WhenInverseDiagonal_ThenXWins() {
        var board = new Board(3, 4, new MNKBoardRuleset());

        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[0, 2].MarkIndex = _playerXIndex;
        board.Spaces[1, 1].MarkIndex = _playerXIndex;
        board.Spaces[2, 0].MarkIndex = _playerXIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }

    [Fact]
    public void Given3x4Board_WhenInverseDiagonalOffset_ThenXWins() {
        var board = new Board(3, 4, new MNKBoardRuleset());

        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[0, 3].MarkIndex = _playerXIndex;
        board.Spaces[1, 2].MarkIndex = _playerXIndex;
        board.Spaces[2, 1].MarkIndex = _playerXIndex;

        board.Spaces[0, 0].MarkIndex = _playerOIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }
    #endregion

    #region 4x6 rectangular boards (W=4, H=6, diagLen=4) - diagonal tests

    [Fact]
    public void Given4x6Board_WhenIdentityDiagonal_ThenXWins() {
        var board = new Board(4, 6, new MNKBoardRuleset());

        // Identity diagonal: starts at (0, H-diagLen) = (0, 2), ends at (diagLen-1, H-1) = (3, 5)
        board.Spaces[0, 0].MarkIndex = _playerXIndex;
        board.Spaces[1, 1].MarkIndex = _playerXIndex;
        board.Spaces[2, 2].MarkIndex = _playerXIndex;
        board.Spaces[3, 3].MarkIndex = _playerXIndex;

        board.Spaces[0, 3].MarkIndex = _playerOIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }

    [Fact]
    public void Given4x6Board_WhenIdentityDiagonalOffset1_ThenXWins() {
        var board = new Board(4, 6, new MNKBoardRuleset());

        // Identity diagonal: starts at (0, H-diagLen) = (0, 2), ends at (diagLen-1, H-1) = (3, 5)
        board.Spaces[0, 1].MarkIndex = _playerXIndex;
        board.Spaces[1, 2].MarkIndex = _playerXIndex;
        board.Spaces[2, 3].MarkIndex = _playerXIndex;
        board.Spaces[3, 4].MarkIndex = _playerXIndex;

        board.Spaces[0, 3].MarkIndex = _playerOIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }


    [Fact]
    public void Given4x6Board_WhenIdentityDiagonalOffset2_ThenXWins_() {
        var board = new Board(4, 6, new MNKBoardRuleset());

        // Identity diagonal: starts at (0, H-diagLen) = (0, 2), ends at (diagLen-1, H-1) = (3, 5)
        board.Spaces[0, 2].MarkIndex = _playerXIndex;
        board.Spaces[1, 3].MarkIndex = _playerXIndex;
        board.Spaces[2, 4].MarkIndex = _playerXIndex;
        board.Spaces[3, 5].MarkIndex = _playerXIndex;

        board.Spaces[0, 3].MarkIndex = _playerOIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }

    [Fact]
    public void Given4x6Board_WhenInverseDiagonal_ThenXWins() {
        var board = new Board(4, 6, new MNKBoardRuleset());

        // Inverse diagonal: starts at (0, diagLen-1) = (0, 3), ends at (diagLen-1, 0) = (3, 0)
        board.Spaces[0, 3].MarkIndex = _playerXIndex;
        board.Spaces[1, 2].MarkIndex = _playerXIndex;
        board.Spaces[2, 1].MarkIndex = _playerXIndex;
        board.Spaces[3, 0].MarkIndex = _playerXIndex;

        board.Spaces[1, 1].MarkIndex = _playerOIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }
    #endregion

    #region ScoreLength tests
    [Fact]
    public void Given6x6BoardScoringLength3_WhenScoringLineIsLength4_ThenXWins1Point() {
        var board = new Board(6, 6, new MNKBoardRuleset(ScoringLength: 3));

        board.Spaces[0, 3].MarkIndex = _playerXIndex;
        board.Spaces[1, 2].MarkIndex = _playerXIndex;
        board.Spaces[2, 1].MarkIndex = _playerXIndex;
        board.Spaces[3, 0].MarkIndex = _playerXIndex;

        board.Spaces[3, 3].MarkIndex = _playerOIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 1));
    }

    [Fact]
    public void Given6x6BoardScoringLength3_WhenScoringLineIsLength6_ThenXWins2Points() {
        var board = new Board(6, 6, new MNKBoardRuleset(ScoringLength: 3));

        board.Spaces[0, 0].MarkIndex = _playerXIndex;
        board.Spaces[1, 1].MarkIndex = _playerXIndex;
        board.Spaces[2, 2].MarkIndex = _playerXIndex;
        board.Spaces[3, 3].MarkIndex = _playerXIndex;
        board.Spaces[4, 4].MarkIndex = _playerXIndex;
        board.Spaces[5, 5].MarkIndex = _playerXIndex;

        board.ExecuteRuleset();

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerIndexScore(_playerXIndex, 2));
    }
    #endregion

    #region Empty boards
    [Fact]
    public void Given3x3Board_WhenEmptyBoard_ThenNoScore() {
        var board = new Board(3, 3, new MNKBoardRuleset());
        board.ExecuteRuleset();

        board.ScoreCard.Highest.Should().Be(ScoreCard.Empty);
    }

    [Fact]
    public void Given4x6Board_WhenEmptyBoard_NoScore() {
        var board = new Board(4, 6, new MNKBoardRuleset());
        board.ExecuteRuleset();

        board.ScoreCard.Highest.Should().Be(ScoreCard.Empty);
    }
    #endregion
    #region impasse
    [Fact]
    public void Given3x3Board_WhenLineOfImpasse_ThenImpasseIsNotScored() {
        var board = new Board(3, 3, new MNKBoardRuleset());

        // A full line of impasses (MarkIndex == -1) is the state that used to
        // be scored as player -1, putting an out-of-range entry in the
        // ScoreCard that broke AIGameRunner.
        for (int row = 0; row < 3; row += 1) {
            board.Spaces[0, row].MarkIndex = Space.ImpasseMarkIndex;
        }

        board.ExecuteRuleset();

        // Impasse spaces are not owned by any player, so they must not be
        // scored.
        board.ScoreCard.PlayerScores
            .Should()
            .NotContain(ps => ps.PlayerIndex == Space.ImpasseMarkIndex);
    }
    #endregion
}
