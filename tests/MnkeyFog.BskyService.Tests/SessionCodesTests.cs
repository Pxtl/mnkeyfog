// SPDX-License-Identifier: MIT

namespace MnkeyFog.BskyService.Tests;

public class SessionCodesTests {
    [Theory]
    [InlineData(1, "B")]
    [InlineData(5, "F")]
    [InlineData(16, "Q")]
    [InlineData(31, "7")]
    [InlineData(32, "BA")]
    [InlineData(33, "BB")]
    [InlineData(1023, "77")]
    [InlineData(1024, "BAA")]
    [InlineData(32767, "777")]
    public void ToBase32_KnownValues_ReturnsExpectedEncoding(int value, string expected) {
        SessionCodes.ToBase32(value).Should().Be(expected);
    }

    [Fact]
    public void ToBase32_Zero_Throws() {
        var act = () => SessionCodes.ToBase32(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ToBase32_Negative_Throws() {
        var act = () => SessionCodes.ToBase32(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("B", 1, 1)]
    [InlineData("F", 5, 1)]
    [InlineData("Q", 16, 1)]
    [InlineData("7", 31, 1)]
    [InlineData("BA", 32, 2)]
    [InlineData("77", 1023, 2)]
    [InlineData("BAA", 1024, 3)]
    [InlineData("777", 32767, 3)]
    public void ToBase32FromBase32_KnownCodes_ReturnsValueAndMatchesCodeLength(string code, int value, int expectedLen) {
        var parsed = SessionCodes.FromBase32(code);
        parsed.Should().Be(value);
        var decoded = SessionCodes.FromBase32(code) ?? 0;
        SessionCodes.ToBase32(decoded).Length.Should().Be(expectedLen);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(31, 1)]
    [InlineData(32, 2)]
    [InlineData(33, 2)]
    [InlineData(1023, 2)]
    [InlineData(1024, 3)]
    [InlineData(1025, 3)]
    [InlineData(32767, 3)]
    [InlineData(32768, 4)]
    [InlineData(32769, 4)]
    public void LengthOfValue_KnownValues_ReturnsDigitCount(int value, int expected) {
        SessionCodes.LengthOfValue(value).Should().Be(expected);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(1023)]
    public void ToBase32_TwoCharRange_HasCorrectLength(int value) {
        // 1..1023 (MaxShortCodeValue) all fit in at most 2 chars.
        SessionCodes.ToBase32(value).Length.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public void ToBase32_ValueAboveMaxShortCode_SpillsToThreeChars() {
        SessionCodes.ToBase32(SessionCodes.MaxShortCodeValue + 1).Length.Should().Be(3);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(1023)]
    [InlineData(1024)]
    [InlineData(32767)]
    [InlineData(123456)]
    public void ToBase32FromBase32_RoundTrips(int value) {
        SessionCodes.FromBase32(SessionCodes.ToBase32(value)).Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("!")]
    [InlineData("0128")]
    [InlineData("A00")]
    public void FromBase32_InvalidOrMissing_ReturnsNull(string? code) {
        SessionCodes.FromBase32(code).Should().BeNull();
    }

    [Fact]
    public void MaxShortCodeValue_Is1023() {
        SessionCodes.MaxShortCodeValue.Should().Be(1023);
    }

    [Fact]
    public void ToBase32_EveryValueInShortRange_RoundTrips() {
        // Exhaustive round-trip over the entire short-code range.
        for (var value = 1; value <= SessionCodes.MaxShortCodeValue; value++) {
            var code = SessionCodes.ToBase32(value);
            code.Length.Should().BeLessThanOrEqualTo(2);
            SessionCodes.FromBase32(code).Should().Be(value, $"value {value}");
        }
    }
}
