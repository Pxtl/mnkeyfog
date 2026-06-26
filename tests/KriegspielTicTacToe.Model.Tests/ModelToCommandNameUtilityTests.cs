namespace KriegspielTicTacToe.Model.Tests;

using Xunit;

public class ModelToCommandNameUtilityTests {
    #region BuildPlayerToCommandNameMap
    [Fact]
    public void BuildPlayerToCommandNameMap_SingleTypeableMark() {
        var map = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(
            new[] { new Player("A") }
        );
        
        map.ContainsKey(new Player("A")).Should().BeTrue();
        map.ContainsKey(new Player("B")).Should().BeFalse();
    }
    
    [Fact]
    public void BuildPlayerToCommandNameMap_WithOnlyTypeableMarks_ReturnsIdentity()
    {
        var marks = new[] { "X", "O", "9", "5" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);

        Assert.Equal(4, result.Count);
        foreach (var player in players) {
            Assert.Equal(player.Mark, result[player]);
        }
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_LowercaseLettersAreIdentity()
    {
        var marks = new[] { "x", "a", "z", "b" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);

        foreach (var player in players) {
            Assert.Equal(player.Mark, result[player]);
        }
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_AlphabeticLettersAreTypeable()
    {
        var marks = new[] { "X", "O", "A", "Z", "B" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);

        foreach (var player in players) {
            Assert.Equal(player.Mark, result[player]);
        }
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_PunctuationMarksAreNonTypeable()
    {
        var marks = new[] { ".", "!", "?", ":", ";" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var expected = new Dictionary<Player, string>{
            [new Player(".")] = "1",
            [new Player("!")] = "2",
            [new Player("?")] = "3",
            [new Player(":")] = "4",
            [new Player(";")] = "5"
        };
        var actual = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_ExcludesUsedMarksFromAlternates()
    {
        var marks = new[] { "A", ".", "!", "1" };
        var players = new List<Player> { };
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }
        
        var expected = new Dictionary<Player, string>{
            [new Player("A")] = "A",
            [new Player(".")] = "2",
            [new Player("!")] = "3",
            [new Player("1")] = "1"
        };
        var actual = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_AllNumbersConsumedUsesLetters()
    {
        var marks = new[] { "1", "2", "3", "4", "5", ".", "!", "?", ":", ";", "/" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var expected = new Dictionary<Player, string>{
            [new Player("1")] = "1",
            [new Player("2")] = "2",
            [new Player("3")] = "3",
            [new Player("4")] = "4",
            [new Player("5")] = "5",
            [new Player(".")] = "6",
            [new Player("!")] = "7",
            [new Player("?")] = "8",
            [new Player(":")] = "9",
            [new Player(";")] = "A",
            [new Player("/")] = "B",
        };
        var actual = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_MultiCharMarkGetsAlternateKey()
    {
        // Multi-char marks cannot be used directly ( ArgumentException), 
        // but we can test they are not typeable by using Player.FromString with null
        var typeableMark = "A";
        var typeablePlayer = new Player(typeableMark);
        
        var players = new List<Player> {
            new Player(typeablePlayer.Mark),
        };
        
        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);
        
        // Single-char typeable mark returns identity
        Assert.Equal(typeableMark, result[typeablePlayer]);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_PreservesPlayerReferencesInDictionary()
    {
        var players = new List<Player> {
            new Player("X"),
            new Player("O")
        };

        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);

        var originalX = players.First(p => p.Mark == "X");
        Assert.True(result.ContainsKey(originalX));
        Assert.Equal("X", result[originalX]);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_ReturnsCorrectCountForEmptyInput()
    {
        var emptyPlayers = new List<Player>();
        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(emptyPlayers);
        Assert.Empty(result);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_DigitsAreTypeable()
    {
        var marks = new[] { "1", "8", "9" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);

        foreach (var player in players) {
            Assert.Equal(player.Mark, result[player]);
        }
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_MixedTypeableAndNonTypeableWorksCorrectly()
    {
        var typeable = new[] { "X", "O" };
        var nonTypeable = new[] { ".", "!" };
        var players = new List<Player>();
        foreach (var t in typeable) {
            players.Add(new Player(t));
        }
        foreach (var nt in nonTypeable) {
            players.Add(new Player(nt));
        }

        var expected = new Dictionary<Player, string>{
            [new Player("X")] = "X",
            [new Player("O")] = "O",
            [new Player(".")] = "1",
            [new Player("!")] = "2"
        };
        var actual = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_TwoSpecialChars_ReturnMap1and2() {
        var player1 = new Player("☃"); //unicode snowman
        var player2 = new Player("☂"); //unicode umbrella
        var map = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(
            new[] { player1, player2 }
        );
        
        map[player1].Should().Be("1");
        map[player2].Should().Be("2");
    }
    #endregion

    #region BuildPlayerToCommandNameMap Edge Cases
    
    [Fact]
    public void BuildPlayerToCommandNameMap_EmptyArray() {
        var map = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(Array.Empty<Player>());
        
        map.Count.Should().Be(0);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_NullValueThrows() {
        var action = () => {
            _ = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(null!);
        };
        action.Should().Throw<ArgumentNullException>();
    }
    #endregion
    
    #region GetSpaceCommandName
    [Fact]
    public void GetSpaceCommandName_Empty3x3BoardYourTurnIsAsExpected() {
        var players = new Player[] {new ("X"), new ("O")};
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3), MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var expected = new string[3,3] {
            {"17", "18", "19"},
            {"14", "15", "16"},
            {"11", "12", "13"}
        };

        for(sbyte row = 0; row < expected.GetLength(0); row += 1) {
            for(sbyte col = 0; col < expected.GetLength(0); col += 1) {
                var actual = ModelToCommandNameUtility.GetSpaceCommandName(new GameView(gameState, players[0]), 0, col, row);
                actual.Should().Be(expected[row, col]);
            }
        }
    }

    [Fact]
    public void GetSpaceCommandName_Empty3x3BoardNotYourTurnIsBlank() {
        var players = new Player[] {new ("X"), new ("O")};
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3), MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var expected = "";

        for(sbyte row = 0; row < gameState.Boards[0].RowCount; row += 1) {
            for(sbyte col = 0; col < gameState.Boards[0].RowCount; col += 1) {
                var actual = ModelToCommandNameUtility.GetSpaceCommandName(new GameView(gameState, players[1]), 0, col, row);
                actual.Should().Be(expected);
            }
        }
    }

    [Fact]
    public void GetSpaceCommandName_SecondRound3x3BoardYourTurnIsAsExpected() {
        var players = new Player[] {new ("X"), new ("O")};
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        gameState.GetView(players[0]).Attempt(new MNKAction(0, 1, 1));
        gameState.GetView(players[1]).Attempt(new MNKAction(0, 1, 1));
        gameState.PlayManager.EndRound(out _);

        var expected = new string[3,3] {
            {"7", "8", "9"},
            {"4", "X", "6"},
            {"1", "2", "3"}
        };

        for(sbyte row = 0; row < expected.GetLength(0); row += 1) {
            for(sbyte col = 0; col < expected.GetLength(0); col += 1) {
                var actual = ModelToCommandNameUtility.GetSpaceCommandName(new GameView(gameState, players[0]), 0, col, row);
                actual.Should().Be(expected[row, col]);
            }
        }
    }

    [Fact]
    public void GetSpaceCommandName_NonKriegspielModeCanSeeOtherPlayer() {
        var players = new Player[] {new ("X"), new ("O")};
        var playerX = players[0];
        var playerO = players[1];
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: false),
            isRandomPlayerOrder: false
        );

        gameState.GetView(playerX).Attempt(new MNKAction(0, 1, 1));

        var expected = new string[3,3] {
            {"7", "8", "9"},
            {"4", "X", "6"},
            {"1", "2", "3"}
        };

        for(sbyte row = 0; row < expected.GetLength(0); row += 1) {
            for(sbyte col = 0; col < expected.GetLength(0); col += 1) {
                var actual = ModelToCommandNameUtility.GetSpaceCommandName(new GameView(gameState, playerO), 0, col, row);
                actual.Should().Be(expected[row, col]);
            }
        }
    }

    [Fact]
    public void GetSpaceCommandName_MoveSameSpaceCanSeeRevealedSpace() {
        var players = new Player[] {new ("X"), new ("O")};
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        //round 1
        gameState.GetView(players[0]).Attempt(new MNKAction(0, 1, 1));
        gameState.GetView(players[1]).Attempt(new MNKAction(0, 1, 1));
        gameState.PlayManager.EndRound(out _);

        var expected = players[0].Mark;
        var actual = ModelToCommandNameUtility.GetSpaceCommandName(new GameView(gameState, players[1]), 0, col: 1, row: 1);
        actual.Should().Be(expected);
    }

    [Fact]
    public void GetSpaceCommandName_MoveDifferentSpaceCantSeeOtherPlayer() {
        var players = new Player[] {new ("X"), new ("O")};
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        //round 1
        gameState.GetView(players[0]).Attempt(new MNKAction(0, 1, 1));
        gameState.GetView(players[1]).Attempt(new MNKAction(0, 0, 0));
        gameState.PlayManager.EndRound(out _);

        var expected = "";
        var actual = ModelToCommandNameUtility.GetSpaceCommandName(new GameView(gameState, players[1]), 0, col: 1, row: 1);
        actual.Should().Be(expected);
    }

    [Fact]
    public void GetSpaceCommandName_ThirdRound3x3SpectatorViewIsAsExpected() {
        var players = new Player[] {new ("X"), new ("O")};
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        //round 1
        gameState.GetView(players[0]).Attempt(new MNKAction(0, 1, 1));
        gameState.GetView(players[1]).Attempt(new MNKAction(0, 1, 1));
        gameState.PlayManager.EndRound(out _);

        //round 2
        gameState.GetView(players[0]).Attempt(new MNKAction(0, 0, 0));
        gameState.GetView(players[1]).Attempt(new MNKAction(0, 2, 2));
        gameState.PlayManager.EndRound(out _);

        var expected = new string[3,3] {
            {"X", " ", " "},
            {" ", "X", " "},
            {" ", " ", "O"}
        };

        for(sbyte row = 0; row < expected.GetLength(0); row += 1) {
            for(sbyte col = 0; col < expected.GetLength(0); col += 1) {
                var actual = ModelToCommandNameUtility.GetSpaceCommandName(new GameView(gameState, null), 0, col, row);
                actual.Should().Be(expected[row, col].Trim());
            }
        }
    }

    [Fact]
    public void GetSpaceCommandName_Empty4x4BoardYourTurnIsAsExpected() {
        var players = new Player[] {new ("X"), new ("O")};
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(4, 4)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var expected = new string[4,4] {
            {"A4", "B4", "C4", "D4"},
            {"A3", "B3", "C3", "D3"},
            {"A2", "B2", "C2", "D2"},
            {"A1", "B1", "C1", "D1"}
        };

        for(sbyte row = 0; row < expected.GetLength(0); row += 1) {
            for(sbyte col = 0; col < expected.GetLength(0); col += 1) {
                var actual = ModelToCommandNameUtility.GetSpaceCommandName(new GameView(gameState, players[0]), 0, col, row);
                actual.Should().Be(expected[row, col]);
            }
        }
    }

    #endregion
}

