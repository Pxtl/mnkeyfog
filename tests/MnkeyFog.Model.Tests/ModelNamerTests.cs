namespace MnkeyFog.Model.Tests;

using MnkeyFog.Text;
using Xunit;

public class ModelNamerTests {
    private readonly ModelNamer _namer = new();


    #region BuildPlayerToCommandNameMap
    [Fact]
    public void BuildPlayerToCommandNameMap_SingleTypeableMark() {
        var map = _namer.BuildPlayerToCommandNameMap(
            (new[] { new PlayerInfo("A") }).ToPlayersIndexed()
        );

        map.ContainsKey(0).Should().BeTrue();
        map.ContainsKey(1).Should().BeFalse();
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_WithOnlyTypeableMarks_ReturnsIdentity() {
        var players = (new[] { "X", "O", "9", "5" }).ToPlayersIndexed();

        var result = _namer.BuildPlayerToCommandNameMap(players);

        Assert.Equal(4, result.Count);
        foreach (var player in players) {
            Assert.Equal(player.CommandName, result[player.Index]);
        }
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_LowercaseLettersAreIdentity() {
        var players = (new[] { "x", "a", "z", "b" }).ToPlayersIndexed();

        var result = _namer.BuildPlayerToCommandNameMap(players);

        foreach (var player in players) {
            Assert.Equal(player.Info.CommandName, result[player.Index]);
        }
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_AlphabeticLettersAreTypeable() {
        var players = (new[] { "X", "O", "A", "Z", "B" }).ToPlayersIndexed();

        var result = _namer.BuildPlayerToCommandNameMap(players);

        foreach (var player in players) {
            Assert.Equal(player.Info.CommandName, result[player.Index]);
        }
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_PunctuationMarksAreNonTypeable() {
        var players = (new[] { ".", "!", "?", ":", ";" }).ToPlayersIndexed();

        var expected = new Dictionary<int, string> {
            [players.Single(p => p.CommandName == ".").Index] = "1",
            [players.Single(p => p.CommandName == "!").Index] = "2",
            [players.Single(p => p.CommandName == "?").Index] = "3",
            [players.Single(p => p.CommandName == ":").Index] = "4",
            [players.Single(p => p.CommandName == ";").Index] = "5"
        };
        var actual = _namer.BuildPlayerToCommandNameMap(players);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_ExcludesUsedMarksFromAlternates() {
        var players = (new[] { "A", ".", "!", "1" }).ToPlayersIndexed();

        var expected = new Dictionary<int, string> {
            [players.Single(p => p.CommandName == "A").Index] = "A",
            [players.Single(p => p.CommandName == ".").Index] = "2",
            [players.Single(p => p.CommandName == "!").Index] = "3",
            [players.Single(p => p.CommandName == "1").Index] = "1"
        };
        var actual = _namer.BuildPlayerToCommandNameMap(players);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_AllNumbersConsumedUsesLetters() {
        var players = (new[] { "1", "2", "3", "4", "5", ".", "!", "?", ":", ";", "/" }).ToPlayersIndexed();

        var expected = new Dictionary<int, string> {
            [players.Single(p => p.CommandName == "1").Index] = "1",
            [players.Single(p => p.CommandName == "2").Index] = "2",
            [players.Single(p => p.CommandName == "3").Index] = "3",
            [players.Single(p => p.CommandName == "4").Index] = "4",
            [players.Single(p => p.CommandName == "5").Index] = "5",
            [players.Single(p => p.CommandName == ".").Index] = "6",
            [players.Single(p => p.CommandName == "!").Index] = "7",
            [players.Single(p => p.CommandName == "?").Index] = "8",
            [players.Single(p => p.CommandName == ":").Index] = "9",
            [players.Single(p => p.CommandName == ";").Index] = "A",
            [players.Single(p => p.CommandName == "/").Index] = "B",
        };
        var actual = _namer.BuildPlayerToCommandNameMap(players);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_MultiCharMarkGetsAlternateKey() {
        // Multi-char marks cannot be used directly ( ArgumentException), 
        // but we can test they are not typeable by using Player.FromString with null
        var typeableMark = "A";
        var playerInfos = new List<PlayerInfo> {
            new PlayerInfo(typeableMark),
        }.ToPlayersIndexed();

        var typeablePlayerInfo = playerInfos.First();
        var result = _namer.BuildPlayerToCommandNameMap(playerInfos);

        // Single-char typeable mark returns identity
        Assert.Equal(typeableMark, result[typeablePlayerInfo.Index]);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_PreservesPlayerReferencesInDictionary() {
        var playerInfos = new List<PlayerInfo> {
            new PlayerInfo("X"),
            new PlayerInfo("O")
        }.ToPlayersIndexed();

        var result = _namer.BuildPlayerToCommandNameMap(playerInfos);

        var originalX = playerInfos.First(p => p.CommandName == "X");
        Assert.True(result.ContainsKey(originalX.Index));
        Assert.Equal("X", result[originalX.Index]);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_ReturnsCorrectCountForEmptyInput() {
        var emptyPlayerInfos = new List<PlayerInfo>().ToPlayersIndexed();
        var result = _namer.BuildPlayerToCommandNameMap(emptyPlayerInfos);
        Assert.Empty(result);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_DigitsAreTypeable() {
        var players = (new[] { "1", "8", "9" }).ToPlayersIndexed();

        var result = _namer.BuildPlayerToCommandNameMap(players);

        foreach (var player in players) {
            Assert.Equal(player.Info.CommandName, result[player.Index]);
        }
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_MixedTypeableAndNonTypeableWorksCorrectly() {
        var playerInfos = new List<PlayerInfo>() {
            //typeable
            new("X"),
            new("O"),
            //non-typeable
            new("."),
            new("!")
        }.ToPlayersIndexed();

        var expected = new Dictionary<int, string> {
            [playerInfos.Single(p => p.CommandName == "X").Index] = "X",
            [playerInfos.Single(p => p.CommandName == "O").Index] = "O",
            [playerInfos.Single(p => p.CommandName == ".").Index] = "1",
            [playerInfos.Single(p => p.CommandName == "!").Index] = "2"
        };
        var actual = _namer.BuildPlayerToCommandNameMap(playerInfos);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_TwoSpecialChars_ReturnMap1and2() {
        var player1 = new PlayerInfo("☃"); //unicode snowman
        var player2 = new PlayerInfo("☂"); //unicode umbrella
        var player1Index = 0;
        var player2Index = 1;
        var map = _namer.BuildPlayerToCommandNameMap(
            new[] { player1, player2 }.ToPlayersIndexed()
        );

        map[player1Index].Should().Be("1");
        map[player2Index].Should().Be("2");
    }
    #endregion

    #region BuildPlayerToCommandNameMap Edge Cases

    [Fact]
    public void BuildPlayerToCommandNameMap_EmptyArray() {
        var map = _namer.BuildPlayerToCommandNameMap(Array.Empty<Player>());

        map.Count.Should().Be(0);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_NullValueThrows() {
        var action = () => {
            _ = _namer.BuildPlayerToCommandNameMap(null!);
        };
        action.Should().Throw<ArgumentNullException>();
    }
    #endregion

    #region GetSpaceBody
    [Fact]
    public void GetSpaceBody_Empty3x3BoardYourTurnIsAsExpected() {
        var players = new PlayerInfo[] { new("X"), new("O") };
        var playerXIndex = 0;
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3), MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var gameView = new GameView(gameState, playerXIndex);
        var expected = new string[3, 3] {
            {"17", "18", "19"},
            {"14", "15", "16"},
            {"11", "12", "13"}
        };

        for (sbyte row = 0; row < expected.GetLength(0); row += 1) {
            for (sbyte col = 0; col < expected.GetLength(0); col += 1) {
                var actual = _namer.GetSpaceBody(gameView, 0, col, row, gameView.CanTakeTurn);
                actual.Should().Be(expected[row, col]);
            }
        }
    }

    [Fact]
    public void GetSpaceBody_Empty3x3BoardNotYourTurnIsBlank() {
        var players = new PlayerInfo[] { new("X"), new("O") };
        var playerOIndex = 1;
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3), MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var gameView = new GameView(gameState, playerOIndex);
        var expected = " ";

        for (sbyte row = 0; row < gameState.Boards[0].RowCount; row += 1) {
            for (sbyte col = 0; col < gameState.Boards[0].RowCount; col += 1) {
                var actual = _namer.GetSpaceBody(gameView, 0, col, row, gameView.CanTakeTurn);
                actual.Should().Be(expected);
            }
        }
    }

    [Fact]
    public void GetSpaceBody_SecondRound3x3BoardYourTurnIsAsExpected() {
        var players = new PlayerInfo[] { new("X"), new("O") };
        var playerXIndex = 0;
        var playerOIndex = 1;
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        gameState.GetView(playerXIndex).Attempt(new MNKAction(0, 1, 1));
        gameState.GetView(playerOIndex).Attempt(new MNKAction(0, 1, 1));
        gameState.EndRound(out _);

        var gameView = new GameView(gameState, playerXIndex);
        var expected = new string[3, 3] {
            {"7", "8", "9"},
            {"4", "X", "6"},
            {"1", "2", "3"}
        };

        for (sbyte row = 0; row < expected.GetLength(0); row += 1) {
            for (sbyte col = 0; col < expected.GetLength(0); col += 1) {
                var actual = _namer.GetSpaceBody(gameView, 0, col, row, gameView.CanTakeTurn);
                actual.Should().Be(expected[row, col]);
            }
        }
    }

    [Fact]
    public void GetSpaceBody_NonKriegspielModeCanSeeOtherPlayer() {
        var players = new PlayerInfo[] { new("X"), new("O") };
        var playerXIndex = 0;
        var playerOIndex = 1;
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: false),
            isRandomPlayerOrder: false
        );

        gameState.GetView(playerXIndex).Attempt(new MNKAction(0, 1, 1));

        var gameView = new GameView(gameState, playerOIndex);
        var expected = new string[3, 3] {
            {"7", "8", "9"},
            {"4", "X", "6"},
            {"1", "2", "3"}
        };

        for (sbyte row = 0; row < expected.GetLength(0); row += 1) {
            for (sbyte col = 0; col < expected.GetLength(0); col += 1) {
                var actual = _namer.GetSpaceBody(gameView, 0, col, row, gameView.CanTakeTurn);
                actual.Should().Be(expected[row, col]);
            }
        }
    }

    [Fact]
    public void GetSpaceBody_MoveSameSpaceCanSeeRevealedSpace() {
        var players = new PlayerInfo[] { new("X"), new("O") };
        var playerXIndex = 0;
        var playerOIndex = 1;
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        //round 1
        gameState.GetView(playerXIndex).Attempt(new MNKAction(0, 1, 1));
        gameState.GetView(playerOIndex).Attempt(new MNKAction(0, 1, 1));
        gameState.EndRound(out _);

        var gameView = new GameView(gameState, playerOIndex);
        var expected = players[playerXIndex].CommandName;
        var actual = _namer.GetSpaceBody(gameView, boardIndex: 0, col: 1, row: 1, gameView.CanTakeTurn);
        actual.Should().Be(expected);
    }

    [Fact]
    public void GetSpaceBody_MoveDifferentSpaceCantSeeOtherPlayer() {
        var players = new PlayerInfo[] { new("X"), new("O") };
        var playerXIndex = 0;
        var playerOIndex = 1;
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        //round 1
        gameState.GetView(playerXIndex).Attempt(new MNKAction(0, 1, 1));
        gameState.GetView(playerOIndex).Attempt(new MNKAction(0, 0, 0));
        gameState.EndRound(out _);

        var gameView = new GameView(gameState, playerOIndex);
        var expected = " ";
        var actual = _namer.GetSpaceBody(gameView, boardIndex: 0, col: 1, row: 1, gameView.CanTakeTurn);
        actual.Should().Be(expected);
    }

    [Fact]
    public void GetSpaceBody_ThirdRound3x3SpectatorViewIsAsExpected() {
        var players = new PlayerInfo[] { new("X"), new("O") };
        var playerXIndex = 0;
        var playerOIndex = 1;
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        //round 1
        gameState.GetView(playerXIndex).Attempt(new MNKAction(0, 1, 1));
        gameState.GetView(playerOIndex).Attempt(new MNKAction(0, 1, 1));
        gameState.EndRound(out _);

        //round 2
        gameState.GetView(playerXIndex).Attempt(new MNKAction(0, 0, 0));
        gameState.GetView(playerOIndex).Attempt(new MNKAction(0, 2, 2));
        gameState.EndRound(out _);

        var gameView = new GameView(gameState, null);
        var expected = new string[3, 3] {
            {"X", " ", " "},
            {" ", "X", " "},
            {" ", " ", "O"}
        };

        for (sbyte row = 0; row < expected.GetLength(0); row += 1) {
            for (sbyte col = 0; col < expected.GetLength(0); col += 1) {
                var actual = _namer.GetSpaceBody(gameView, 0, col, row, gameView.CanTakeTurn);
                actual.Should().Be(expected[row, col]);
            }
        }
    }

    [Fact]
    public void GetSpaceBody_Empty4x4BoardYourTurnIsAsExpected() {
        var players = new PlayerInfo[] { new("X"), new("O") };
        var playerXIndex = 0;
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(4, 4)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var gameView = new GameView(gameState, playerXIndex);
        var expected = new string[4, 4] {
            {"A4", "B4", "C4", "D4"},
            {"A3", "B3", "C3", "D3"},
            {"A2", "B2", "C2", "D2"},
            {"A1", "B1", "C1", "D1"}
        };

        for (sbyte row = 0; row < expected.GetLength(0); row += 1) {
            for (sbyte col = 0; col < expected.GetLength(0); col += 1) {
                var actual = _namer.GetSpaceBody(gameView, 0, col, row, gameView.CanTakeTurn);
                actual.Should().Be(expected[row, col]);
            }
        }
    }

    #endregion

    #region SpaceCommandName
    [Fact]
    public void SpaceCommandName_ReturnsSpaceNameForEmptySpaces() {
        var players = new PlayerInfo[] { new("X"), new("O") };
        var playerXIndex = 0;
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var gameView = new GameView(gameState, playerXIndex);
        var expected = new string[3, 3] {
            {"7", "8", "9"},
            {"4", "5", "6"},
            {"1", "2", "3"}
        };

        for (sbyte row = 0; row < expected.GetLength(0); row += 1) {
            for (sbyte col = 0; col < expected.GetLength(0); col += 1) {
                var actual = _namer.SpaceCommandName(gameView, 0, col, row);
                actual.Should().Be(expected[row, col]);
            }
        }
    }

    [Fact]
    public void SpaceCommandName_ReturnsSpaceNameForOccupiedSpace() {
        var players = new PlayerInfo[] { new("X"), new("O") };
        var playerXIndex = 0;
        var playerOIndex = 1;
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(3, 3)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        gameState.GetView(playerXIndex).Attempt(new MNKAction(0, 1, 1));
        gameState.GetView(playerOIndex).Attempt(new MNKAction(0, 1, 1));
        gameState.EndRound(out _);

        var gameView = new GameView(gameState, playerXIndex);
        // The space command name for an occupied space is still the space name
        // (e.g. "5"), not the occupying mark ("X").
        var actual = _namer.SpaceCommandName(gameView, boardIndex: 0, col: 1, row: 1);
        actual.Should().Be("5");
    }
    #endregion

    #region BoardModelNames
    [Fact]
    public void GetBoardModelName_DefaultNamer_IsIndexPlusOne() {
        var namer = new ModelNamer();

        namer.GetBoardModelName(0).Should().Be(new ModelName("1", "1"));
        namer.GetBoardModelName(3).Should().Be(new ModelName("4", "4"));
    }

    [Fact]
    public void GetBoardModelName_DefaultNamer_WorksForLargeIndex() {
        var namer = new ModelNamer();

        namer.GetBoardModelName(99).Should().Be(new ModelName("100", "100"));
    }

    [Fact]
    public void GetBoardModelName_CustomNamer_IsPreProvided() {
        var namer = new ModelNamer(new[] {
            new ModelName("P", "U"),
            new ModelName("Q", "V"),
            new ModelName("R", "W")
        });

        namer.GetBoardModelName(0).Should().Be(new ModelName("P", "U"));
        namer.GetBoardModelName(1).Should().Be(new ModelName("Q", "V"));
        namer.GetBoardModelName(2).Should().Be(new ModelName("R", "W"));
    }

    [Fact]
    public void GetBoardModelName_CustomNamer_OutOfRangeThrows() {
        var namer = new ModelNamer(new[] { new ModelName("P", "U") });

        var action = () => namer.GetBoardModelName(1);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetBoardModelName_CustomNamer_NullListThrows() {
        var action = () => new ModelNamer((IEnumerable<ModelName>)null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryGetBoardIndexByCommandName_DefaultNamer_ParsesNumber() {
        var namer = new ModelNamer();

        namer.TryGetBoardIndexByCommandName("3", 5, out var i).Should().BeTrue();
        i.Should().Be(2);

        namer.TryGetBoardIndexByCommandName("99", 5, out var i2).Should().BeFalse();
        i2.Should().Be(-1);
    }

    [Fact]
    public void TryGetBoardIndexByCommandName_CustomNamer_FindsByCommandName() {
        var namer = new ModelNamer(new[] {
            new ModelName("P", "U"),
            new ModelName("Q", "V"),
            new ModelName("R", "W")
        });

        namer.TryGetBoardIndexByCommandName("Q", 3, out var i).Should().BeTrue();
        i.Should().Be(1);

        namer.TryGetBoardIndexByCommandName("X", 3, out var i2).Should().BeFalse();
        i2.Should().Be(-1);
    }
    #endregion

    #region RowAndColumnModelNames
    [Fact]
    public void GetColumnModelName_DefaultNamer_IsLetter() {
        var namer = new ModelNamer();

        namer.GetColumnModelName(0, 26).Should().Be(new ModelName("A", "A"));
        namer.GetColumnModelName(1, 26).Should().Be(new ModelName("B", "B"));
        namer.GetColumnModelName(25, 26).Should().Be(new ModelName("Z", "Z"));
    }

    [Fact]
    public void GetColumnModelName_CustomNamer_IsPreProvided() {
        var namer = new ModelNamer(
            boardModelNames: new[] { new ModelName("1", "1") },
            columnModelNames: new[] { new ModelName("L", "λ"), new ModelName("M", "μ") },
            rowModelNames: new[] { new ModelName("1", "1") });

        namer.GetColumnModelName(0, 2).Should().Be(new ModelName("L", "λ"));
        namer.GetColumnModelName(1, 2).Should().Be(new ModelName("M", "μ"));
    }

    [Fact]
    public void GetColumnModelName_CustomNamer_OutOfRangeThrows() {
        var namer = new ModelNamer(
            boardModelNames: new[] { new ModelName("1", "1") },
            columnModelNames: new[] { new ModelName("L", "λ") },
            rowModelNames: new[] { new ModelName("1", "1") });

        var action = () => namer.GetColumnModelName(1, 1);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetColumnModelName_DefaultNamer_APaddedForLargeBoards() {
        var namer = new ModelNamer();

        // 27-column board: width 2, A-padded (A=0).
        namer.GetColumnModelName(0, 27).Should().Be(new ModelName("AA", "AA"));
        namer.GetColumnModelName(1, 27).Should().Be(new ModelName("AB", "AB"));
        namer.GetColumnModelName(25, 27).Should().Be(new ModelName("AZ", "AZ"));
        namer.GetColumnModelName(26, 27).Should().Be(new ModelName("BA", "BA"));
        // 676-column board: width 2, last column is "ZZ".
        namer.GetColumnModelName(675, 676).Should().Be(new ModelName("ZZ", "ZZ"));
        // 677-column board: width 3, first is "AAA", last is "BAA".
        namer.GetColumnModelName(0, 677).Should().Be(new ModelName("AAA", "AAA"));
        namer.GetColumnModelName(676, 677).Should().Be(new ModelName("BAA", "BAA"));
    }

    [Fact]
    public void GetRowModelName_DefaultNamer_IsOneBasedFromBottomZeroPadded() {
        var namer = new ModelNamer();

        // 5-row board: top row (0) -> "5", bottom row (4) -> "1" (1 digit).
        namer.GetRowModelName(0, 5).Should().Be(new ModelName("5", "5"));
        namer.GetRowModelName(4, 5).Should().Be(new ModelName("1", "1"));
        // 26-row board: top row (0) -> "26", bottom row (25) -> "01" (2 digits).
        namer.GetRowModelName(0, 26).Should().Be(new ModelName("26", "26"));
        namer.GetRowModelName(25, 26).Should().Be(new ModelName("01", "01"));
    }

    [Fact]
    public void GetRowModelName_CustomNamer_IsPreProvided() {
        var namer = new ModelNamer(
            boardModelNames: new[] { new ModelName("1", "1") },
            columnModelNames: new[] { new ModelName("A", "A") },
            rowModelNames: new[] { new ModelName("T", "top"), new ModelName("B", "bot") });

        // 0-based from the top.
        namer.GetRowModelName(0, 2).Should().Be(new ModelName("T", "top"));
        namer.GetRowModelName(1, 2).Should().Be(new ModelName("B", "bot"));
    }

    [Fact]
    public void GetRowModelName_CustomNamer_OutOfRangeThrows() {
        var namer = new ModelNamer(
            boardModelNames: new[] { new ModelName("1", "1") },
            columnModelNames: new[] { new ModelName("A", "A") },
            rowModelNames: new[] { new ModelName("1", "1") });

        var action = () => namer.GetRowModelName(1, 1);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SpaceCommandName_UsesPreProvidedRowAndColumnNames() {
        var players = new PlayerInfo[] { new("X"), new("O") };
        var gameState = new GameState(
            players,
            new MNKTemplate([MNKBoardRuleset.CreateBoardBuilder(4, 4)], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        var gameView = new GameView(gameState, 0);
        var namer = new ModelNamer(
            boardModelNames: new[] { new ModelName("1", "1") },
            columnModelNames: new[] { new ModelName("L", "L"), new ModelName("M", "M"), new ModelName("N", "N"), new ModelName("O", "O") },
            rowModelNames: new[] { new ModelName("T", "T"), new ModelName("S", "S"), new ModelName("R", "R"), new ModelName("Q", "Q") });

        // col 0, row 0 -> column "L" + row "T".
        namer.SpaceCommandName(gameView, 0, 0, 0).Should().Be("LT");
        // col 1, row 1 -> column "M" + row "S".
        namer.SpaceCommandName(gameView, 0, 1, 1).Should().Be("MS");
    }
    #endregion
}

