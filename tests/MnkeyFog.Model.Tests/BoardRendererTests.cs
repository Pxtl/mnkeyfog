namespace MnkeyFog.Model.Tests;

#pragma warning disable IDE0027 // Simplify nested expression
public class BoardRendererTests {
    
    [Fact]
    public void DrawBoards_3x3_ReturnsBlankBoardGridString() {
        var boardBuilder = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        var state = new GameState(
            new[] { 'X', 'O' }.ToPlayersArray(),
            new MNKTemplate([ boardBuilder ], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        
        var currentPlayer = state.PlayManager.PlayersAvailableForTurn.First();
        state.PlayManager.EndTurn(currentPlayer, out _);
        
        var actual = BoardRenderer.DrawBoards(new GameView(state, currentPlayer));
        var expected = @"
  ┌───┬───┬───┐
  │   │   │   │
  ├───┼───┼───┤
  │   │   │   │
  ├───┼───┼───┤
  │   │   │   │
  └───┴───┴───┘
  "
            .Substring(Environment.NewLine.Length) //skip the leading linebreak needed for legibility
            .TrimEnd()
            .ReplaceLineEndings();

        actual.TrimEnd().Should().Be(expected);
    }

    [Fact]
    public void DrawBoards_3x3WithOneMove_ReturnsBoardGridStringWithMove() {
        var boardBuilder = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate([ boardBuilder ], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        
        var currentPlayer = new Player("X");
        state.GetView(currentPlayer).Attempt(new MNKAction(0, 0, 0));
        var expected = @"
  ┌───┬───┬───┐
  │ X │   │   │
  ├───┼───┼───┤
  │   │   │   │
  ├───┼───┼───┤
  │   │   │   │
  └───┴───┴───┘
  "
            .Substring(Environment.NewLine.Length) //skip the leading linebreak needed for legibility
            .TrimEnd()
            .ReplaceLineEndings();

        var actual = BoardRenderer.DrawBoards(new GameView(state, currentPlayer));    
        actual.TrimEnd().Should().Be(expected);
    }


    [Fact]
    public void DrawBoards_3x3WithActiveBoard_ReturnBoardsWithSpaceCodesGridString() {
        var boardBuilder3x3 = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate([ boardBuilder3x3 ], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var currentPlayer = new Player("X");
        // 0 means wrap as tight as possible.
        var actual = BoardRenderer.DrawBoards(new GameView(state, currentPlayer));

        var expected = @"
  ┌───┬───┬───┐
  │ 7 │ 8 │ 9 │
  ├───┼───┼───┤
  │ 4 │ 5 │ 6 │
  ├───┼───┼───┤
  │ 1 │ 2 │ 3 │
  └───┴───┴───┘
"
            .Substring(Environment.NewLine.Length) //skip the leading linebreak needed for legibility
            .TrimEnd()
            .ReplaceLineEndings();

        actual.TrimEnd().Should().Be(expected);
    }

    [Fact]
    public void DrawBoards_3x3WithActiveBoardAndOneMove_ReturnBoardsWithSpaceCodesGridString() {
        var boardBuilder3x3 = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate([ boardBuilder3x3 ], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var currentPlayer = new Player("X");
        state.GetView(currentPlayer).Attempt(new MNKAction(0, 1, 1));     
        var otherPlayer = new Player("O");
        state.GetView(otherPlayer).Attempt(new MNKAction(0, 0, 0));
        state.PlayManager.EndRound(out var _);

        var actual = BoardRenderer.DrawBoards(new GameView(state, currentPlayer));

        var expected = @"
  ┌───┬───┬───┐
  │ 7 │ 8 │ 9 │
  ├───┼───┼───┤
  │ 4 │ X │ 6 │
  ├───┼───┼───┤
  │ 1 │ 2 │ 3 │
  └───┴───┴───┘
"
            .Substring(Environment.NewLine.Length) //skip the leading linebreak needed for legibility
            .TrimEnd()
            .ReplaceLineEndings();

        actual.TrimEnd().Should().Be(expected);
    }

    [Fact]
    public void DrawBoards_MaxSizeReturnsBlankBoardGridString() {
        var boardBuilder = MNKBoardRuleset.CreateBoardBuilder(26,26);
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate([ boardBuilder ], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        
        var currentPlayer = state.PlayManager.PlayersAvailableForTurn.First();
        state.PlayManager.EndTurn(currentPlayer, out _);
        
        var actual = BoardRenderer.DrawBoards(new GameView(state, currentPlayer));
        var expected = @"
  ┌───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┐
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │   │
  └───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┘
"
            .Substring(Environment.NewLine.Length) //skip the leading linebreak needed for legibility
            .TrimEnd()
            .ReplaceLineEndings();

        actual.TrimEnd().Should().Be(expected);
    }

    [Fact]
    public void DrawBoards_MaxSizeWithActiveBoard_ReturnBoardsWithSpaceCodesGridString() {
        var boardBuilder = MNKBoardRuleset.CreateBoardBuilder(26, 26);
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate([ boardBuilder ], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );
        
        var currentPlayer = state.PlayManager.PlayersAvailableForTurn.First();
        
        var actual = BoardRenderer.DrawBoards(new GameView(state, currentPlayer));
        var expected = @"
  ┌───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┐
  │A26│B26│C26│D26│E26│F26│G26│H26│I26│J26│K26│L26│M26│N26│O26│P26│Q26│R26│S26│T26│U26│V26│W26│X26│Y26│Z26│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A25│B25│C25│D25│E25│F25│G25│H25│I25│J25│K25│L25│M25│N25│O25│P25│Q25│R25│S25│T25│U25│V25│W25│X25│Y25│Z25│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A24│B24│C24│D24│E24│F24│G24│H24│I24│J24│K24│L24│M24│N24│O24│P24│Q24│R24│S24│T24│U24│V24│W24│X24│Y24│Z24│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A23│B23│C23│D23│E23│F23│G23│H23│I23│J23│K23│L23│M23│N23│O23│P23│Q23│R23│S23│T23│U23│V23│W23│X23│Y23│Z23│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A22│B22│C22│D22│E22│F22│G22│H22│I22│J22│K22│L22│M22│N22│O22│P22│Q22│R22│S22│T22│U22│V22│W22│X22│Y22│Z22│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A21│B21│C21│D21│E21│F21│G21│H21│I21│J21│K21│L21│M21│N21│O21│P21│Q21│R21│S21│T21│U21│V21│W21│X21│Y21│Z21│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A20│B20│C20│D20│E20│F20│G20│H20│I20│J20│K20│L20│M20│N20│O20│P20│Q20│R20│S20│T20│U20│V20│W20│X20│Y20│Z20│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A19│B19│C19│D19│E19│F19│G19│H19│I19│J19│K19│L19│M19│N19│O19│P19│Q19│R19│S19│T19│U19│V19│W19│X19│Y19│Z19│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A18│B18│C18│D18│E18│F18│G18│H18│I18│J18│K18│L18│M18│N18│O18│P18│Q18│R18│S18│T18│U18│V18│W18│X18│Y18│Z18│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A17│B17│C17│D17│E17│F17│G17│H17│I17│J17│K17│L17│M17│N17│O17│P17│Q17│R17│S17│T17│U17│V17│W17│X17│Y17│Z17│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A16│B16│C16│D16│E16│F16│G16│H16│I16│J16│K16│L16│M16│N16│O16│P16│Q16│R16│S16│T16│U16│V16│W16│X16│Y16│Z16│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A15│B15│C15│D15│E15│F15│G15│H15│I15│J15│K15│L15│M15│N15│O15│P15│Q15│R15│S15│T15│U15│V15│W15│X15│Y15│Z15│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A14│B14│C14│D14│E14│F14│G14│H14│I14│J14│K14│L14│M14│N14│O14│P14│Q14│R14│S14│T14│U14│V14│W14│X14│Y14│Z14│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A13│B13│C13│D13│E13│F13│G13│H13│I13│J13│K13│L13│M13│N13│O13│P13│Q13│R13│S13│T13│U13│V13│W13│X13│Y13│Z13│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A12│B12│C12│D12│E12│F12│G12│H12│I12│J12│K12│L12│M12│N12│O12│P12│Q12│R12│S12│T12│U12│V12│W12│X12│Y12│Z12│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A11│B11│C11│D11│E11│F11│G11│H11│I11│J11│K11│L11│M11│N11│O11│P11│Q11│R11│S11│T11│U11│V11│W11│X11│Y11│Z11│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A10│B10│C10│D10│E10│F10│G10│H10│I10│J10│K10│L10│M10│N10│O10│P10│Q10│R10│S10│T10│U10│V10│W10│X10│Y10│Z10│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A09│B09│C09│D09│E09│F09│G09│H09│I09│J09│K09│L09│M09│N09│O09│P09│Q09│R09│S09│T09│U09│V09│W09│X09│Y09│Z09│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A08│B08│C08│D08│E08│F08│G08│H08│I08│J08│K08│L08│M08│N08│O08│P08│Q08│R08│S08│T08│U08│V08│W08│X08│Y08│Z08│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A07│B07│C07│D07│E07│F07│G07│H07│I07│J07│K07│L07│M07│N07│O07│P07│Q07│R07│S07│T07│U07│V07│W07│X07│Y07│Z07│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A06│B06│C06│D06│E06│F06│G06│H06│I06│J06│K06│L06│M06│N06│O06│P06│Q06│R06│S06│T06│U06│V06│W06│X06│Y06│Z06│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A05│B05│C05│D05│E05│F05│G05│H05│I05│J05│K05│L05│M05│N05│O05│P05│Q05│R05│S05│T05│U05│V05│W05│X05│Y05│Z05│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A04│B04│C04│D04│E04│F04│G04│H04│I04│J04│K04│L04│M04│N04│O04│P04│Q04│R04│S04│T04│U04│V04│W04│X04│Y04│Z04│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A03│B03│C03│D03│E03│F03│G03│H03│I03│J03│K03│L03│M03│N03│O03│P03│Q03│R03│S03│T03│U03│V03│W03│X03│Y03│Z03│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A02│B02│C02│D02│E02│F02│G02│H02│I02│J02│K02│L02│M02│N02│O02│P02│Q02│R02│S02│T02│U02│V02│W02│X02│Y02│Z02│
  ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
  │A01│B01│C01│D01│E01│F01│G01│H01│I01│J01│K01│L01│M01│N01│O01│P01│Q01│R01│S01│T01│U01│V01│W01│X01│Y01│Z01│
  └───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┘
"
            .Substring(Environment.NewLine.Length) //skip the leading linebreak needed for legibility
            .TrimEnd()
            .ReplaceLineEndings();

        actual.TrimEnd().Should().Be(expected);
    }

    [Fact]
    public void DrawBoards_3x3MultipleBoardsWithWrapping_ReturnWrappedBoardGridString() {
        var boardBuilder3x3 = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate([ boardBuilder3x3, boardBuilder3x3, boardBuilder3x3 ], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var currentPlayer = new Player("X");
        state.PlayManager.EndTurn(currentPlayer, out _);

        // wrap halfway through 3rd board
        var actual = BoardRenderer.DrawBoards(new GameView(state, currentPlayer), maxRenderWidth:42);
        var expected = @"
 1┌───┬───┬───┐ 2┌───┬───┬───┐
  │   │   │   │  │   │   │   │
  ├───┼───┼───┤  ├───┼───┼───┤
  │   │   │   │  │   │   │   │
  ├───┼───┼───┤  ├───┼───┼───┤
  │   │   │   │  │   │   │   │
  └───┴───┴───┘  └───┴───┴───┘
 3┌───┬───┬───┐
  │   │   │   │
  ├───┼───┼───┤
  │   │   │   │
  ├───┼───┼───┤
  │   │   │   │
  └───┴───┴───┘
  "
            .Substring(Environment.NewLine.Length) //skip the leading linebreak needed for legibility
            .TrimEnd()
            .ReplaceLineEndings();

        actual.TrimEnd().Should().Be(expected);
    }

    [Fact]
    public void DrawBoards_3x3MultipleBoardsWithNarrowWrapping_ReturnWrappedBoardGridString() {
        var boardBuilder3x3 = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate([ boardBuilder3x3, boardBuilder3x3, boardBuilder3x3 ], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var currentPlayer = new Player("X");
        state.PlayManager.EndTurn(currentPlayer, out _);
        
        // 0 means wrap as tight as possible.
        var actual = BoardRenderer.DrawBoards(new GameView(state, currentPlayer), maxRenderWidth: 0);
        var expected = @"
 1┌───┬───┬───┐
  │   │   │   │
  ├───┼───┼───┤
  │   │   │   │
  ├───┼───┼───┤
  │   │   │   │
  └───┴───┴───┘
 2┌───┬───┬───┐
  │   │   │   │
  ├───┼───┼───┤
  │   │   │   │
  ├───┼───┼───┤
  │   │   │   │
  └───┴───┴───┘
 3┌───┬───┬───┐
  │   │   │   │
  ├───┼───┼───┤
  │   │   │   │
  ├───┼───┼───┤
  │   │   │   │
  └───┴───┴───┘
  "
            .Substring(Environment.NewLine.Length) //skip the leading linebreak needed for legibility
            .TrimEnd()
            .ReplaceLineEndings();

        actual.TrimEnd().Should().Be(expected);
    }

    [Fact]
    public void DrawBoards_3x3MultipleBoardsWithActiveBoard_ReturnBoardsWithSpaceCodesGridString() {
        var boardBuilder3x3 = MNKBoardRuleset.CreateBoardBuilder(3, 3);
        
        var state = new GameState(
            (new[] { 'X', 'O' }).ToPlayersArray(),
            new MNKTemplate([ boardBuilder3x3, boardBuilder3x3, boardBuilder3x3 ], isSynchronousMode: false, isKriegspiel: true),
            isRandomPlayerOrder: false
        );

        var currentPlayer = new Player("X");

        var actual = BoardRenderer.DrawBoards(new GameView(state, currentPlayer), maxRenderWidth: 999999);

        var expected = @"
 1┌───┬───┬───┐ 2┌───┬───┬───┐ 3┌───┬───┬───┐
  │17 │18 │19 │  │27 │28 │29 │  │37 │38 │39 │
  ├───┼───┼───┤  ├───┼───┼───┤  ├───┼───┼───┤
  │14 │15 │16 │  │24 │25 │26 │  │34 │35 │36 │
  ├───┼───┼───┤  ├───┼───┼───┤  ├───┼───┼───┤
  │11 │12 │13 │  │21 │22 │23 │  │31 │32 │33 │
  └───┴───┴───┘  └───┴───┴───┘  └───┴───┴───┘
"
            .Substring(Environment.NewLine.Length) //skip the leading linebreak needed for legibility
            .TrimEnd()
            .ReplaceLineEndings();

        actual.TrimEnd().Should().Be(expected);
    }
}
#pragma warning restore IDE0027 // Simplify nested expression
