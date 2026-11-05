// SPDX-License-Identifier: MIT

namespace MnkeyFog.BskyService;

/// <summary>
/// Session-code generation. Codes are the standard (RFC 4648) unpadded base32
/// encoding of the sequence value: value 1 → "B", value 32 → "BA", value 33
/// → "BB". One character covers values 0–31 and two characters cover values
/// 32–1023 (<see cref="MaxShortCodeValue"/>); value 0 ("A") is unused.
/// </summary>
public static class SessionCodes {
    /// <summary>
    /// Largest sequence value that still encodes to two or fewer base32
    /// characters. One char covers 0–31, two chars cover 32–1023, so three
    /// characters begin at 1024.
    /// </summary>
    public const int MaxShortCodeValue = 1023;

    /// <summary>
    /// Encode a positive sequence value (counting from 1): value 1 → "B", …,
    /// value 1023 → "77", value 1024 → "BAA".
    /// </summary>
    /// <param name="value">Sequence value, at least 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="value" /> is less than 1.</exception>
    public static string ToBase32(int value) {
        ThrowIfNotPositive(value);
        return Base32.Encode(value);
    }

    /// <summary>
    /// Decode a session code (emitted by <see cref="ToBase32"/>) back to its
    /// sequence value, or null if it is not valid base32 (empty, or contains a
    /// character outside the base32 alphabet, ignoring case).
    /// </summary>
    public static int? FromBase32(string? code) {
        return Base32.Decode(code);
    }

    /// <summary>
    /// Number of base32 characters a value needs. Value 0 and negatives return 0.
    /// </summary>
    public static int LengthOfValue(int value) {
        if (value < 1) {
            return 0;
        }
        return Base32.LengthOfValue(value);
    }

    private static void ThrowIfNotPositive(int value) {
        if (value < 1) {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be at least 1.");
        }
    }
}
