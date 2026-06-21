namespace KriegspielTicTacToe.Model.Tests;

using FluentAssertions;
using KriegspielTicTacToe.Model;
using OneOf.Types;

/// <summary>
/// Tests for ModelToCommandNameUtility static class.
/// </summary>
public class ModelToCommandNameUtilityTests {

    #region Typeable Mark Tests
    
    [Fact]
    public void BuildPlayerToCommandNameMap_TypeableLetter_A() {
        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(
            new[] { new Player("A") }
        );
        
        result.ContainsKey(new Player("A")).Should().BeTrue();
        result[new Player("A")]?.Equals("A").Should().BeTrue();
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_TypeableLetter_B_C() {
        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(
            new[] { 
                new Player("B"), 
                new Player("C") 
            }
        );

        result[new Player("B")]?.Equals("B").Should().BeTrue();
        result[new Player("C")]?.Equals("C").Should().BeTrue();
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_TypeableLetter_X_Y_Z() {
        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(
            new[] { 
                new Player("X"), 
                new Player("Y"), 
                new Player("Z") 
            }
        );

        result[new Player("X")]?.Equals("X").Should().BeTrue();
        result[new Player("Y")]?.Equals("Y").Should().BeTrue();
        result[new Player("Z")]?.Equals("Z").Should().BeTrue();
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_TypeableDigit_0() {
        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(
            new[] { 
                new Player("A"),
                new Player("O"),
                new Player("0")  // digit zero, not letter O
            }
        );

        result[new Player("A")]?.Equals("A").Should().BeTrue();
        result[new Player("O")]?.IsNullOrEmpty().Should().BeTrue(); // can't use "O" (confused with 0)
        result[new Player("0")]?.Equals("0").Should().BeTrue();
    }

    #endregion
    
    #region Non-Typeable Mark Tests
    
    [Fact]
    public void BuildPlayerToCommandNameMap_EmptyMark_AssignsFirstDigit() {
        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(
            new[] { 
                new Player(""),  // empty mark
                new Player("X")
            }
        );

        result[new Player("")]?.Equals("1").Should().BeTrue();  // assigned first available digit
        result[new Player("X")]?.Equals("X").Should().BeTrue();
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_EmojiMark_AssignsLetter() {
        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(
            new[] { 
                new Player("😊"),  // emoji mark
                new Player("X")
            }
        );

        result[new Player("😊")]?.Equals("A").Should().BeTrue();  // assigned first available letter 'A'
        result[new Player("X")]?.Equals("X").Should().BeTrue();
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_MixedTypeableAndEmoji_AssignsAlternates() {
        var players = new[] { 
            new Player("😊"),  // emoji - needs alternate key (will get '1')
            new Player("0"),    // digit zero - will get 'A' (O blocked)
            new Player("A"),    // letter A
            new Player("X")     // letter X
        };

        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);
        
        result[new Player("0")]?.Equals("0").Should().BeTrue();
        result[new Player("A")]?.Equals("A").Should().BeTrue();
        result[new Player("X")]?.Equals("X").Should().BeTrue();
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_TypeableLetter_O_Zero_Conflict() {
        var players = new[] { 
            new Player("O"),   // letter O
            new Player("0")    // digit zero  
        };

        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);
        
        result[new Player("O")]?.Equals("O").Should().BeTrue();
        result[new Player("0")]?.Equals("0").Should().BeTrue();
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_MultipleEmoji_FirstGetsDigit() {
        var players = new[] { 
            new Player("😊"),  // emoji mark - will get '1' or another unused digit/letter
            new Player("😄"),  // another emoji - gets next available key
            new Player("X")    
        };

        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);
        
        result[new Player("😊")]?.IsSingleCharDigit().Should().BeTrue();    // should get a digit if unused digits exist
        result[new Player("😄")]?.Equals("X").Should().BeFalse();           // emoji gets alternate, not X (letter)
    }

    #endregion
    
    #region Edge Case Tests
    
    [Fact]
    public void BuildPlayerToCommandNameMap_SingleNonTypeableMark_AssignsOneDigit() {
        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(
            new[] { 
                new Player("🎲")  // game spinner emoji
            }
        );
        
        result[new Player("🎲")]?.Length.Should().Be(1);
    }

    [Fact]
    public void BuildPlayerToCommandNameMap_AllTypeable_UsesMarksDirectly() {
        var players = new[] { 
            new Player("A"), 
            new Player("B"), 
            new Player("C"),
            new Player("D")
        };
        
        var result = ModelToCommandNameUtility.BuildPlayerToCommandNameMap(players);

        foreach (var player in players) {
            result[player]?.Equals(player.Mark).Should().BeTrue();
        }
    }

    #endregion
    
}
