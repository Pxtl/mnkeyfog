namespace KriegspielTicTacToe.Model.Tests;

using FluentAssertions;
using Xunit;

public class ScoringTests {
    #region orthogonal tests
    
    [Fact]
    public void ScoreCard_CalculatesWinningBoard_XWins() {
        var board = new Board(3, 3);

        board.Spaces[0, 0].Mark = "X";
        board.Spaces[0, 1].Mark = "X";
        board.Spaces[0, 2].Mark = "X";

        board.ScoreCard.HighestScore.Should().NotBeNull();
        if(board.ScoreCard.HighestScore.HasValue)
            board.ScoreCard.HighestScore.Value.Player.Mark.Should().Be("X");
    }

    [Fact]
    public void ScoreCard_CalculatesWinningBoardMoreLines_OWins() {
        var board = new Board(3, 3);

        board.Spaces[0, 0].Mark = "X";
        board.Spaces[1, 0].Mark = "X";
        board.Spaces[2, 0].Mark = "X";

        board.Spaces[0, 1].Mark = "O";
        board.Spaces[1, 1].Mark = "O";
        board.Spaces[2, 1].Mark = "O";

        board.Spaces[0, 2].Mark = "O";
        board.Spaces[1, 2].Mark = "O";
        board.Spaces[2, 2].Mark = "O";

        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore.Value.Should().Be(new PlayerScore("O", 2));
    }
    #endregion
    #region 3x3 square boards - baseline diagonal tests
    
    [Fact]
    public void ScoreCard_3x3_Diagonal_X_Wins_Identity() {
        var board = new Board(3, 3);
        
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 2].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }

    [Fact]
    public void ScoreCard_3x3_Diagonal_X_Wins_Inverse() {
        var board = new Board(3, 3);
        
        board.Spaces[0, 2].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 0].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }
    #endregion

    #region 4x3 rectangular boards (W=4, H=3, diagLen=3) - diagonal tests
    
    [Fact]
    public void ScoreCard_4x3_Diagonal_X_Wins_Identity() {
        var board = new Board(4, 3);
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 0), ends at (diagLen-1, H-1) = (2, 2)
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 2].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }

    [Fact]
    public void ScoreCard_4x3_Diagonal_X_Wins_OffsetIdentity() {
        var board = new Board(4, 3);
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 0), ends at (diagLen-1, H-1) = (2, 2)
        board.Spaces[1, 0].Mark = "X";
        board.Spaces[2, 1].Mark = "X";
        board.Spaces[3, 2].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }

    [Fact]
    public void ScoreCard_4x3_Diagonal_X_Wins_Inverse() {
        var board = new Board(4, 3);
        
        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[0, 2].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 0].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }

        [Fact]
    public void ScoreCard_4x3_Diagonal_X_Wins_InverseOffset() {
        var board = new Board(4, 3);
        
        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[1, 2].Mark = "X";
        board.Spaces[2, 1].Mark = "X";
        board.Spaces[3, 0].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }

    [Fact]
    public void ScoreCard_4x3_DiagonalEdgeToEdgeOnly2InLine_NoScore() {
        var board = new Board(4, 3);
        
        // Only 2 X's inline on diagonal - not a winning line
        board.Spaces[0, 1].Mark = "X";
        board.Spaces[1, 2].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().BeNull();
    }
    #endregion

    #region 3x4 rectangular boards (W=3, H=4, diagLen=3) - diagonal tests
    
    [Fact]
    public void ScoreCard_3x4_Diagonal_X_Wins_Identity() {
        var board = new Board(3, 4);
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 1), ends at (diagLen-1, H-1) = (2, 3)
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 2].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }

    [Fact]
    public void ScoreCard_3x4_Diagonal_X_Wins_IdentityOffset() {
        var board = new Board(3, 4);
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 1), ends at (diagLen-1, H-1) = (2, 3)
        board.Spaces[0, 1].Mark = "X";
        board.Spaces[1, 2].Mark = "X";
        board.Spaces[2, 3].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }

    [Fact]
    public void ScoreCard_3x4_Diagonal_X_Wins_Inverse() {
        var board = new Board(3, 4);
        
        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[0, 2].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 0].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }

    [Fact]
    public void ScoreCard_3x4_Diagonal_X_Wins_InverseOffset() {
        var board = new Board(3, 4);
        
        // Inverse diagonal: starts at (0, diagLen-1) = (0, 2), ends at (diagLen-1, 0) = (2, 0)
        board.Spaces[0, 3].Mark = "X";
        board.Spaces[1, 2].Mark = "X";
        board.Spaces[2, 1].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }
    #endregion

    #region 4x6 rectangular boards (W=4, H=6, diagLen=4) - diagonal tests

    [Fact]
    public void ScoreCard_4x6_Diagonal_X_Wins_Identity() {
        var board = new Board(4, 6);
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 2), ends at (diagLen-1, H-1) = (3, 5)
        board.Spaces[0, 0].Mark = "X";
        board.Spaces[1, 1].Mark = "X";
        board.Spaces[2, 2].Mark = "X";
        board.Spaces[3, 3].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }

    [Fact]
    public void ScoreCard_4x6_Diagonal_X_Wins_IdentityOffset1() {
        var board = new Board(4, 6);
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 2), ends at (diagLen-1, H-1) = (3, 5)
        board.Spaces[0, 1].Mark = "X";
        board.Spaces[1, 2].Mark = "X";
        board.Spaces[2, 3].Mark = "X";
        board.Spaces[3, 4].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }
    
    
    [Fact]
    public void ScoreCard_4x6_Diagonal_X_Wins_IdentityOffset2() {
        var board = new Board(4, 6);
        
        // Identity diagonal: starts at (0, H-diagLen) = (0, 2), ends at (diagLen-1, H-1) = (3, 5)
        board.Spaces[0, 2].Mark = "X";
        board.Spaces[1, 3].Mark = "X";
        board.Spaces[2, 4].Mark = "X";
        board.Spaces[3, 5].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }

    [Fact]
    public void ScoreCard_4x6_Diagonal_X_Wins_Inverse() {
        var board = new Board(4, 6);
        
        // Inverse diagonal: starts at (0, diagLen-1) = (0, 3), ends at (diagLen-1, 0) = (3, 0)
        board.Spaces[0, 3].Mark = "X";
        board.Spaces[1, 2].Mark = "X";
        board.Spaces[2, 1].Mark = "X";
        board.Spaces[3, 0].Mark = "X";
        
        board.ScoreCard.HighestScore.Should().NotBeNull();
        board.ScoreCard.HighestScore!.Value.Player.Mark.Should().Be("X");
    }
    #endregion

    #region Empty boards
    
    [Fact]
    public void ScoreCard_EmptyBoard_NoScore() {
        var board = new Board(4, 6);
        
        board.ScoreCard.HighestScore.Should().BeNull();
    }

    [Fact]
    public void ScoreCard_EmptyBoard_NoScore_Square() {
        var board = new Board(3, 3);
        
        board.ScoreCard.HighestScore.Should().BeNull();
    }
    #endregion
}
