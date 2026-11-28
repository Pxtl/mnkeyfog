// SPDX-License-Identifier: MIT

namespace MnkeyFog.Model.Tests;

public class MNKTemplateTests {
    [Fact]
    public void TraditionalTemplate_TimeoutActionCommandName_IsNull() {
        GameTemplates.BasicTicTacToe.TimeoutActionCommandName.Should().BeNull();
    }

    [Fact]
    public void CasualTemplate_TimeoutActionCommandName_IsPass() {
        GameTemplates.FogTicTacToe.TimeoutActionCommandName.Should().Be("pass");
    }

    [Fact]
    public void AllBuiltInTemplates_TimeoutActionCommandName_MatchesTemplateKind() {
        foreach (var template in GameTemplates.GetBuiltInGameTemplates()) {
            var isCasual = template is CasualMNKTemplate;
            template.TimeoutActionCommandName.Should().Be(isCasual ? "pass" : null);
        }
    }
}
