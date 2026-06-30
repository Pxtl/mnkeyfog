namespace MnkeyFog.Model.Tests;
public class PlayerTests {
    [Fact]
    public void NewPlayer_EmptyStringThrows() {
        var action = () => {
            _ = new Player("");
        };
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewPlayer_WhitespaceStringThrows() {
        var action = () => {
            _ = new Player(" ");
        };
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewPlayer_NullValueThrows() {
        var action = () => {
            _ = new Player(null!);
        };
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NewPlayer_ControlCharacterThrows() {
        var action = () => {
            _ = new Player("\a"); //bell character, is not considered whitespace
        };
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewPlayer_AlphaCharIsAllowedMark() {
        var expectedMark = "M";
        var actual = new Player(expectedMark);
        actual.Mark.Should().Be(expectedMark);
    }
    
    [Fact]
    public void NewPlayer_SpecialCharIsAllowedMark() {
        var expectedMark = "☂"; //unicode umbrella
        var actual = new Player(expectedMark);
        actual.Mark.Should().Be(expectedMark);
    }
}