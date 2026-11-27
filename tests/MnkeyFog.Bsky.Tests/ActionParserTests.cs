namespace MnkeyFog.Bsky.Tests;

public class ActionParserTests {
    private static readonly ActionParser _parser = new(new ModelNamer());

    private static CommandToken? Parse(string message, bool isPrefixOptional)
        => _parser.ParseCommand(message, isPrefixOptional).Match<CommandToken?>(c => c, _ => null);

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

    private static GameState NewCasualState()
        => new(
            new[] { new PlayerInfo("X"), new PlayerInfo("O") },
            new CasualMNKTemplate(
                "3x3",
                "3x3 casual",
                [2],
                [new BoardBuilder(3, 3, new MNKBoardRuleset(ScoringLength: null, IsBoardDoneWhenScored: true))],
                isKriegspiel: false,
                isSynchronousMode: false
            ),
            isRandomPlayerOrder: false
        );

    private static GameState NewSpaceOnlyState()
        => new(
            new[] { new PlayerInfo("X"), new PlayerInfo("O") },
            GameTemplates.BasicTicTacToe,
            isRandomPlayerOrder: false
        );

    [Fact]
    public void ParsePlay_NamedSpaceAction_CreatesMoves() {
        var state = NewCasualState();
        var view = state.GetView(state.PlayersState.GetPlayer("X"));
        var result = _parser.ParsePlay(view, ["space", "5"]);
        // OneOf: T0 = IReadOnlyList<GameAction> (moves), T1 = CommandError (error).
        result.IsT1.Should().BeFalse();
        result.AsT0.Should().HaveCount(1);
        result.AsT0[0].Should().BeOfType<MNKAction>();
    }

    [Fact]
    public void ParsePlay_BareSpaceTokens_DefaultToSpace() {
        var state = NewCasualState();
        var view = state.GetView(state.PlayersState.GetPlayer("X"));
        var result = _parser.ParsePlay(view, ["5"]);
        // OneOf: T0 = IReadOnlyList<GameAction> (moves), T1 = CommandError (error).
        result.IsT1.Should().BeFalse();
        result.AsT0.Should().HaveCount(1);
        result.AsT0[0].Should().BeOfType<MNKAction>();
    }

    [Fact]
    public void ParsePlay_PassAction_CreatesPass() {
        var state = NewCasualState();
        var view = state.GetView(state.PlayersState.GetPlayer("X"));
        var result = _parser.ParsePlay(view, ["pass"]);
        // OneOf: T0 = IReadOnlyList<GameAction> (moves), T1 = CommandError (error).
        result.IsT1.Should().BeFalse();
        result.AsT0.Should().HaveCount(1);
        result.AsT0[0].Should().BeOfType<PassAction>();
    }

    [Fact]
    public void ParsePlay_PassOnSpaceOnlyGame_InvalidAction() {
        var state = NewSpaceOnlyState();
        var view = state.GetView(state.PlayersState.GetPlayer("X"));
        var result = _parser.ParsePlay(view, ["pass"]);
        // OneOf: T0 = IReadOnlyList<GameAction> (moves), T1 = CommandError (error).
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Contain("Invalid action 'pass'");
    }
}
