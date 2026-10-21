// SPDX-License-Identifier: MIT

namespace MnkeyFog.BskyService.Tests;

#pragma warning disable IDE0027 // Simplify nested expression
public class SessionCodesTests {
    [Theory]
    [InlineData(0, "")]
    [InlineData(1, "B")]
    [InlineData(5, "F")]
    [InlineData(16, "Q")]
    [InlineData(31, "7")]
    [InlineData(32, "BA")]
    [InlineData(1023, "77")]
    [InlineData(1024, "BAA")]
    [InlineData(32767, "777")]
    public void ToBase32_KnownValues_ReturnsExpectedEncoding(int value, string expected) {
        SessionCodes.ToBase32(value).Should().Be(expected);
    }

    [Theory]
    [InlineData("B", 1, 1)]
    [InlineData("F", 5, 1)]
    [InlineData("Q", 16, 1)]
    [InlineData("7", 31, 1)]
    [InlineData("BA", 32, 2)]
    [InlineData("77", 1023, 2)]
    [InlineData("777", 32767, 3)]
    public void ToBase32FromBase32_KnownCodes_ReturnsValueAndMatchesCodeLength(string code, int value, int expectedLen) {
        // FromBase32 always succeeds on these well-formed strings; the decoded
        // value must match the code's digit count and round-trip via ToBase32.
        var parsed = SessionCodes.FromBase32(code);
        parsed.Should().Be(value);
        var decoded = SessionCodes.FromBase32(code) ?? 0;
        SessionCodes.ToBase32(decoded).Length.Should().Be(expectedLen);
        decoded.Should().Be(value);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(31, 1)]
    [InlineData(32, 2)]
    [InlineData(33, 2)]
    [InlineData(1023, 2)]
    [InlineData(1024, 3)]
    [InlineData(32767, 3)]
    [InlineData(32768, 4)]
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
        // Values 1..31 are one char, 32..1023 are two chars. 1..1023 is
        // MaxShortCodeValue, the last value before spilling to three chars.
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
    public void RandomLongCode_DefaultLength_IsSixCharsFromAlphabet() {
        var codes = Enumerable.Range(0, 10)
            .Select(_ => SessionCodes.RandomLongCode())
            .ToList();

        codes.Should().AllSatisfy(code => {
            code.Length.Should().Be(SessionCodes.LongCodeLength);
            code.ToCharArray().Should().AllSatisfy(c => c.Should().BeOneOf(SessionCodes.Alphabet));
        });
    }

    [Fact]
    public void RandomLongCode_10KIterations_IsUniqueEnough() {
        // 10k draws from a 32^6 code space (33.5 billion) are effectively
        // guaranteed to be unique; assert none collide.
        var seen = new HashSet<string>();
        for (var i = 0; i < 10_000; i++) {
            seen.Add(SessionCodes.RandomLongCode());
        }
        seen.Count.Should().Be(10_000);
    }
}
#pragma warning restore IDE0027