namespace MnkeyFog.BskyService.Tests;

public class ActionParserTests {
    private static CommandToken? Parse(string message, bool isPrefixOptional)
        => ActionParser.ParseCommand(message, isPrefixOptional).Match<CommandToken?>(c => c, _ => null);

    [Fact]
    public void ParseCommand_SkipsLeadingMention() {
        var cmd = Parse("@bot.handle !new 3x3 2", isPrefixOptional: false);
        cmd!.Command.Should().Be("new");
        cmd.Arg.Should().Equal("3x3", "2");
    }

    [Fact]
    public void ParseCommand_SkipsMultipleMentions() {
        var cmd = Parse("@bot @other !join 5F", isPrefixOptional: false);
        cmd!.Command.Should().Be("join");
        cmd.Arg.Should().Equal("5F");
    }

    [Fact]
    public void ParseCommand_MentionOnly_IsNone() {
        Parse("@bot", isPrefixOptional: false).Should().BeNull();
    }

    [Fact]
    public void ParseCommand_PrefixWithoutMention_StillWorks() {
        var cmd = Parse("!board", isPrefixOptional: false);
        cmd!.Command.Should().Be("board");
    }
}
