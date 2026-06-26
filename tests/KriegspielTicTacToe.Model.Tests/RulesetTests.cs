namespace KriegspielTicTacToe.Model.Tests;

public class RulesetTests {
    #region orthogonal tests
    
    [Fact]
    public void Given3x3Board_WhenVerticalFull_ThenXWins() {
        var board = new Board(3, 3, new MNKBoardRuleset());

        board.Spaces[0, 0].Mark = "X";
        board.Spaces[0, 1].Mark = "X";
        board.Spaces[0, 2].Mark = "X";

        var expectedPlayerScore = new PlayerScore("X", 1);
        board.ScoreCard.Highest!.Should().Be(expectedPlayerScore);
        board.IsDone.Should().BeFalse();
    }

    [Fact]
    public void Given3x3Board_WhenLineFullAndBoardIsDoneWhenScored_ThenBoardIsDone() {
        var board = new Board(3, 3, new MNKBoardRuleset(IsBoardDoneWhenScored: true));

        board.Spaces[0, 0].Mark = "X";
        board.Spaces[0, 1].Mark = "X";
        board.Spaces[0, 2].Mark = "X";

        var expectedPlayerScore = new PlayerScore("X", 1);
        board.ScoreCard.Highest!.Should().Be(expectedPlayerScore);
        board.IsDone.Should().BeTrue();
    }

    [Fact]
    public void Given3x3Board_WhenLineFullAndBoardIsNotDoneWhenScored_ThenBoardIsNotDone() {
        var board = new Board(3, 3, new MNKBoardRuleset(IsBoardDoneWhenScored: false));

        board.Spaces[0, 0].Mark = "X";
        board.Spaces[0, 1].Mark = "X";
        board.Spaces[0, 2].Mark = "X";

        var expectedPlayerScore = new PlayerScore("X", 1);
        board.ScoreCard.Highest!.Should().Be(expectedPlayerScore);
        board.IsDone.Should().BeFalse();
    }

    [Fact]
    public void Given3x3Board_WhenHorizontalFullWithMultipleWinningRows_ThenMajorityWins() {
        var board = new Board(3, 3, new MNKBoardRuleset());

        board.Spaces[0, 0].Mark = "X";
        board.Spaces[1, 0].Mark = "X";
        board.Spaces[2, 0].Mark = "X";

        board.Spaces[0, 1].Mark = "O";
        board.Spaces[1, 1].Mark = "O";
        board.Spaces[2, 1].Mark = "O";

        board.Spaces[0, 2].Mark = "O";
        board.Spaces[1, 2].Mark = "O";
        board.Spaces[2, 2].Mark = "O";

        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("O", 2));
        board.IsDone.Should().BeTrue();
    }
    #endregion
    #region 3x3 square boards - baseline diagonal tests
    
    [Fact]
    public void Given3x3Board_WhenIdentityDiagonal_ThenXWins() {
        var board = new Board(3, 3, new MNKBoardRuleset());
        
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 2].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }

    [Fact]
    public void Given3x3Board_WhenInverseDiagonal_ThenXWins() {
        var board = new Board(3, 3, new MNKBoardRuleset());
        
        board.Spaces[0, 2].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 0].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }
    #endregion

    #region 4x3 rectangular boards (W=4, H=3, diagLen=3) - diagonal tests
    
    [Fact]
    public void Given4x3Board_WhenIdentityDiagonal_ThenXWins() {
        var board = new Board(4, 3, new MNKBoardRuleset());
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 0), ends at (diagLen-1, H-1) = (2, 2)
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 2].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }

    [Fact]
    public void Given4x3Board_WhenIdentityDiagonalOffset_ThenXWins() {
        var board = new Board(4, 3, new MNKBoardRuleset());
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 0), ends at (diagLen-1, H-1) = (2, 2)
        board.Spaces[1, 0].Mark = "X";
        board.Spaces[2, 1].Mark = "X";
        board.Spaces[3, 2].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }

    [Fact]
    public void Given4x3Board_WhenInverseDiagonal_ThenXWins() {
        var board = new Board(4, 3, new MNKBoardRuleset());
        
        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[0, 2].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 0].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }

        [Fact]
    public void Given4x3Board_WhenInverseDiagonalOffset_ThenXWins() {
        var board = new Board(4, 3, new MNKBoardRuleset());
        
        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[1, 2].Mark = "X";
        board.Spaces[2, 1].Mark = "X";
        board.Spaces[3, 0].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }

    [Fact]
    public void Given4x3Board_WhenDiagonalEdgeToEdgeOnly2InLine_ThenNoScore() {
        var board = new Board(4, 3, new MNKBoardRuleset(IsBoardDoneWhenScored: true));
        
        // Only 2 X's inline on diagonal - not a winning line
        board.Spaces[0, 1].Mark = "X";
        board.Spaces[1, 2].Mark = "X";
        
        board.ScoreCard.Highest.Should().Be(ScoreCard.Empty);
        board.IsDone.Should().BeFalse();
    }
    #endregion

    #region 3x4 rectangular boards (W=3, H=4, diagLen=3) - diagonal tests
    
    [Fact]
    public void Given3x4Board_WhenIdentityDiagonal_ThenXWins() {
        var board = new Board(3, 4, new MNKBoardRuleset());
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 1), ends at (diagLen-1, H-1) = (2, 3)
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 2].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }

    [Fact]
    public void Given3x4Board_WhenIdentityDiagonalOffset_ThenXWins() {
        var board = new Board(3, 4, new MNKBoardRuleset());
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 1), ends at (diagLen-1, H-1) = (2, 3)
        board.Spaces[0, 1].Mark = "X";
        board.Spaces[1, 2].Mark = "X";
        board.Spaces[2, 3].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }

    [Fact]
    public void Given3x4Board_WhenInverseDiagonal_ThenXWins() {
        var board = new Board(3, 4, new MNKBoardRuleset());
        
        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[0, 2].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 0].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }

    [Fact]
    public void Given3x4Board_WhenInverseDiagonalOffset_ThenXWins() {
        var board = new Board(3, 4, new MNKBoardRuleset());
        
        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[0, 3].Mark = "X";
        board.Spaces[1, 2].Mark = "X";
        board.Spaces[2, 1].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Player.Mark.Should().Be("X");
    }
    #endregion

    #region 4x6 rectangular boards (W=4, H=6, diagLen=4) - diagonal tests

    [Fact]
    public void Given4x6Board_WhenIdentityDiagonal_ThenXWins() {
        var board = new Board(4, 6, new MNKBoardRuleset());
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 2), ends at (diagLen-1, H-1) = (3, 5)
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 2].Mark = "X";
        board.Spaces[3, 3].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }

    [Fact]
    public void Given4x6Board_WhenIdentityDiagonalOffset1_ThenXWins() {
        var board = new Board(4, 6, new MNKBoardRuleset());
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 2), ends at (diagLen-1, H-1) = (3, 5)
        board.Spaces[0, 1].Mark = "X";
        board.Spaces[1, 2].Mark = "X";
        board.Spaces[2, 3].Mark = "X";
        board.Spaces[3, 4].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }
    
    
    [Fact]
    public void Given4x6Board_WhenIdentityDiagonalOffset2_ThenXWins_() {
        var board = new Board(4, 6, new MNKBoardRuleset());
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 2), ends at (diagLen-1, H-1) = (3, 5)
        board.Spaces[0, 2].Mark = "X";
        board.Spaces[1, 3].Mark = "X";
        board.Spaces[2, 4].Mark = "X";
        board.Spaces[3, 5].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }

    [Fact]
    public void Given4x6Board_WhenInverseDiagonal_ThenXWins() {
        var board = new Board(4, 6, new MNKBoardRuleset());
        
        // Inverse diagonal: starts at (0, diagLen-1) = (0, 3), ends at (diagLen-1, 0) = (3, 0)
        board.Spaces[0, 3].Mark = "X";
        board.Spaces[1, 2].Mark = "X";
        board.Spaces[2, 1].Mark = "X";
        board.Spaces[3, 0].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }
    #endregion

    #region ScoreLength tests
    [Fact]
    public void Given6x6BoardScoringLength3_WhenScoringLineIsLength4_ThenXWins1Point() {
        var board = new Board(6, 6, new MNKBoardRuleset(ScoringLength: 3));
        
        board.Spaces[0, 3].Mark = "X";
        board.Spaces[1, 2].Mark = "X";
        board.Spaces[2, 1].Mark = "X";
        board.Spaces[3, 0].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 1));
    }

    [Fact]
    public void Given6x6BoardScoringLength3_WhenScoringLineIsLength6_ThenXWins2Points() {
        var board = new Board(6, 6, new MNKBoardRuleset(ScoringLength: 3));
        
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 2].Mark = "X";
        board.Spaces[3, 3].Mark = "X";
        board.Spaces[4, 4].Mark = "X";
        board.Spaces[5, 5].Mark = "X";
        
        board.ScoreCard.Highest.PlayerScores.Single().Should().Be(new PlayerScore("X", 2));
    }
    #endregion

    #region Empty boards
    [Fact]
    public void Given3x3Board_WhenEmptyBoard_ThenNoScore() {
        var board = new Board(3, 3, new MNKBoardRuleset());
        
        board.ScoreCard.Highest.Should().Be(ScoreCard.Empty);
    }
    
    [Fact]
    public void Given4x6Board_WhenEmptyBoard_NoScore() {
        var board = new Board(4, 6, new MNKBoardRuleset());
        
        board.ScoreCard.Highest.Should().Be(ScoreCard.Empty);
    }
    #endregion
}
