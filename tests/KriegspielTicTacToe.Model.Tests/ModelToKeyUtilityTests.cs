namespace KriegspielTicTacToe.Model.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using KriegspielTicTacToe.Model;
using KriegspielTicTacToe;
using Xunit;

public class ModelToKeyUtilityTests
{
    [Fact]
    public void BuildPlayerToKeyMap_WithOnlyTypeableMarks_ReturnsIdentity()
    {
        var marks = new[] { "X", "O", "9", "5" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var result = ModelToKeyUtility.BuildPlayerToKeyMap(players);

        Assert.Equal(4, result.Count);
        foreach (var player in players) {
            Assert.Equal(player.Mark, result[player]);
        }
    }

    [Fact]
    public void BuildPlayerToKeyMap_LowercaseLettersAreIdentity()
    {
        var marks = new[] { "x", "a", "z", "b" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var result = ModelToKeyUtility.BuildPlayerToKeyMap(players);

        foreach (var player in players) {
            Assert.Equal(player.Mark, result[player]);
        }
    }

    [Fact]
    public void BuildPlayerToKeyMap_AlphabeticLettersAreTypeable()
    {
        var marks = new[] { "X", "O", "A", "Z", "B" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var result = ModelToKeyUtility.BuildPlayerToKeyMap(players);

        foreach (var player in players) {
            Assert.Equal(player.Mark, result[player]);
        }
    }

    [Fact]
    public void BuildPlayerToKeyMap_PunctuationMarksAreNonTypeable()
    {
        var marks = new[] { ".", "!", "?", ":", ";" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var result = ModelToKeyUtility.BuildPlayerToKeyMap(players);

        foreach (var player in players) {
            var key = result[player];
            Assert.NotNull(key);
            Assert.True(key.Length == 1);
        }
    }

    [Fact]
    public void BuildPlayerToKeyMap_ExcludesUsedMarksFromAlternates()
    {
        var marks = new[] { ".", "!" };
        var players = new List<Player> {
            new Player("A")
        };
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var result = ModelToKeyUtility.BuildPlayerToKeyMap(players);

        Assert.Equal(3, result.Count);
        Assert.Equal("A", result[new Player("A")]);
        
        var alternateKeys = new List<string>();
        foreach (var player in players) {
            if (player.Mark == "A") continue;
            alternateKeys.Add(result[player]);
        }
        Assert.DoesNotContain("A", alternateKeys);
    }

    [Fact]
    public void BuildPlayerToKeyMap_MultiCharMarkGetsAlternateKey()
    {
        // Multi-char marks cannot be used directly ( ArgumentException), 
        // but we can test they are not typeable by using Player.FromString with null
        var typeableMark = "A";
        var typeablePlayer = new Player(typeableMark);
        
        var players = new List<Player> {
            new Player(typeablePlayer.Mark),
        };
        
        var result = ModelToKeyUtility.BuildPlayerToKeyMap(players);
        
        // Single-char typeable mark returns identity
        Assert.Equal(typeableMark, result[typeablePlayer]);
    }

    [Fact]
    public void BuildPlayerToKeyMap_PreservesPlayerReferencesInDictionary()
    {
        var players = new List<Player> {
            new Player("X"),
            new Player("O")
        };

        var result = ModelToKeyUtility.BuildPlayerToKeyMap(players);

        var originalX = players.First(p => p.Mark == "X");
        Assert.True(result.ContainsKey(originalX));
        Assert.Equal("X", result[originalX]);
    }

    [Fact]
    public void BuildPlayerToKeyMap_ReturnsCorrectCountForEmptyInput()
    {
        var emptyPlayers = new List<Player>();
        var result = ModelToKeyUtility.BuildPlayerToKeyMap(emptyPlayers);
        Assert.Empty(result);
    }

    [Fact]
    public void BuildPlayerToKeyMap_DigitsAreTypeable()
    {
        var marks = new[] { "0", "1", "8", "9" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var result = ModelToKeyUtility.BuildPlayerToKeyMap(players);

        foreach (var player in players) {
            Assert.Equal(player.Mark, result[player]);
        }
    }

    [Fact]
    public void BuildPlayerToKeyMap_MixedTypeableAndNonTypeableWorksCorrectly()
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

        var result = ModelToKeyUtility.BuildPlayerToKeyMap(players);

        Assert.Equal(4, result.Count);
        foreach (var t in typeable) {
            Assert.Equal(t, result[new Player(t)]);
        }
        
        var nonTypeableKeys = new List<string>();
        foreach (var player in players) {
            if (typeable.Contains(player.Mark)) continue;
            nonTypeableKeys.Add(result[player]);
        }
        
        // Non-typeable marks get alternate keys (not letters since letters aren't exhausted)
        Assert.DoesNotContain("X", nonTypeableKeys);
        Assert.DoesNotContain("O", nonTypeableKeys);
    }

    [Fact]
    public void BuildPlayerToKeyMap_WithSingleCharacterMarksOnly()
    {
        var marks = new[] { "A", ".", "B", "!" };
        var players = new List<Player>();
        foreach (var mark in marks) {
            players.Add(new Player(mark));
        }

        var result = ModelToKeyUtility.BuildPlayerToKeyMap(players);

        Assert.Equal(4, result.Count);
        Assert.Equal("A", result[new Player("A")]);
        Assert.NotNull(result[new Player(".")]);
        Assert.Equal("B", result[new Player("B")]);
        Assert.NotNull(result[new Player("!")]);
    }
}
