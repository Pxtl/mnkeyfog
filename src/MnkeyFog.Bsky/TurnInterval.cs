// SPDX-License-Identifier: MIT

using System.Text;

namespace MnkeyFog.Bsky;

/// <summary>
/// A turn interval expressed in a compact suffixed-number form (e.g.
/// <c>1d2h3m</c> = 26 hours and 3 minutes). The same string is used to display
/// a game's interval and to receive it from the user when creating a game.
/// </summary>
public readonly record struct TurnInterval(TimeSpan Value) {
    private static readonly char[] Units = ['w', 'd', 'h', 'm', 's'];
    private static readonly long[] UnitSeconds = [7 * 24 * 60 * 60, 24 * 60 * 60, 60 * 60, 60, 1];

    /// <summary>The default turn interval: 20 minutes.</summary>
    public static readonly TurnInterval Default = new(TimeSpan.FromMinutes(20));

    /// <summary>
    /// Parse a suffixed-number interval (case-insensitive; e.g. <c>1d2h3m</c>,
    /// <c>90m</c>, <c>2h</c>). Units are <c>w</c> (weeks), <c>d</c> (days),
    /// <c>h</c> (hours), <c>m</c> (minutes), <c>s</c> (seconds). Units must appear
    /// in canonical order (weeks before days before …) and each at most once; a
    /// bare number without a unit is rejected.
    /// </summary>
    public static bool TryParse(string? input, out TurnInterval interval) {
        interval = default;
        if (string.IsNullOrWhiteSpace(input)) {
            return false;
        }

        var text = input.Trim();
        var totalSeconds = 0L;
        var lastIndex = -1;
        var i = 0;
        while (i < text.Length) {
            var digitsStart = i;
            while (i < text.Length && char.IsDigit(text[i])) {
                i++;
            }
            if (i == digitsStart) {
                return false; // no digits
            }
            if (!int.TryParse(text.AsSpan(digitsStart, i - digitsStart), out var number)) {
                return false;
            }
            if (i >= text.Length) {
                return false; // no unit
            }
            var unit = char.ToLowerInvariant(text[i]);
            var unitIndex = Array.IndexOf(Units, unit);
            if (unitIndex < 0 || unitIndex <= lastIndex) {
                return false; // unknown, out-of-order, or repeated unit
            }
            lastIndex = unitIndex;
            totalSeconds += number * UnitSeconds[unitIndex];
            i++;
        }
        interval = new TurnInterval(TimeSpan.FromSeconds(totalSeconds));
        return true;
    }

    /// <summary>
    /// Format a <see cref="TimeSpan"/> as a suffixed-number interval (e.g.
    /// <c>1d2h3m</c>). Non-zero components are shown from the largest; a zero
    /// value formats as <c>0s</c>.
    /// </summary>
    public static string Format(TimeSpan value) {
        if (value < TimeSpan.Zero) {
            value = TimeSpan.Zero;
        }
        var sb = new StringBuilder();
        AppendIf(sb, value.Days / 7, 'w');
        AppendIf(sb, value.Days % 7, 'd');
        AppendIf(sb, value.Hours, 'h');
        AppendIf(sb, value.Minutes, 'm');
        AppendIf(sb, value.Seconds, 's');
        return sb.Length > 0 ? sb.ToString() : "0s";
    }

    private static void AppendIf(StringBuilder sb, int value, char unit) {
        if (value > 0) {
            sb.Append(value).Append(unit);
        }
    }

    public override string ToString() => Format(Value);
}
