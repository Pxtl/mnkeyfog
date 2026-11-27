namespace MnkeyFog.Model.Tests;

public class PlayerInfoTests {
    [Fact]
    public void NewPlayer_EmptyStringThrows() {
        var action = () => {
            _ = new PlayerInfo("");
        };
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewPlayer_WhitespaceStringThrows() {
        var action = () => {
            _ = new PlayerInfo(" ");
        };
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewPlayer_NullValueThrows() {
        var action = () => {
            _ = new PlayerInfo(null!);
        };
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NewPlayer_ControlCharacterThrows() {
        var action = () => {
            _ = new PlayerInfo("\a"); //bell character, is not considered whitespace
        };
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewPlayer_AlphaCharIsAllowedMark() {
        var expectedMark = "M";
        var actual = new PlayerInfo(expectedMark);
        actual.CommandName.Should().Be(expectedMark);
    }

    [Fact]
    public void NewPlayer_SpecialCharIsAllowedMark() {
        var expectedMark = "☂"; //unicode umbrella
        var actual = new PlayerInfo(expectedMark);
        actual.CommandName.Should().Be(expectedMark);
    }
}
