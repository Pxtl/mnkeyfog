// SPDX-License-Identifier: MIT

namespace MnkeyFog.BskyService;

/// <summary>
/// Short session-code generation. Codes are an unpadded RFC 4648 base32
/// encoding of a sequence number (A-Z2-7).
///
/// Counting starts at 1 (not 0), so values 1-1023 encode to at most two
/// base32 characters (1..31 are one char, 32..1023 are two chars). Beyond
/// <see cref="MaxShortCodeValue"/> the caller recycles codes from completed
/// sessions rather than growing to three characters immediately, or falls
/// back to longer random codes when nothing recyclable is available.
/// </summary>
public static class SessionCodes {
    /// <summary>RFC 4648 base32 alphabet (A-Z, 2-7), no '=' padding character.</summary>
    public static readonly char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

    /// <summary>
    /// Largest sequence value that still encodes to exactly two base32
    /// characters. One-char codes cover 1-31, two-char codes cover 32-1023, so
    /// three characters begin at 1024 (32*32-1 = 1023).
    /// </summary>
    public const int MaxShortCodeValue = 1023;

    /// <summary>Default length of the longer fallback codes.</summary>
    public const int LongCodeLength = 6;

    /// <summary>
    /// Encode a non-negative integer as an unpadded base32 string.
    /// </summary>
    /// <param name="value">0 encodes to the empty string.</param>
    public static string ToBase32(int value) {
        ThrowIfNegative(value);
        if (value == 0) {
            return string.Empty;
        }

        var chars = new char[32];
        var length = 0;
        while (value > 0) {
            chars[length] = Alphabet[value & 0x1F];
            value >>= 5;
            length++;
        }
        // Least-significant character was written first; flip for output.
        Array.Reverse(chars, 0, length);
        return new string(chars, 0, length);
    }

    /// <summary>
    /// Decode an unpadded base32 string back to its value, or null if it is not
    /// valid base32 (empty, or contains a character outside
    /// <see cref="Alphabet"/>, ignoring case).
    /// </summary>
    public static int? FromBase32(string? code) {
        if (string.IsNullOrEmpty(code)) {
            return null;
        }

        int value = 0;
        foreach (var raw in code.ToUpperInvariant()) {
            var index = Array.IndexOf(Alphabet, raw);
            if (index < 0) {
                return null;
            }
            value = (value << 5) | index;
        }
        return value;
    }

    /// <summary>
    /// Number of base32 characters a value needs (0 encodes to the empty string).
    /// This is the integer digit-count, i.e. the length of the result of
    /// <see cref="ToBase32"/>. One character carries values 1-31, two characters
    /// carry 32-1023, three carry 1024-32767, and so on.
    /// </summary>
    public static int LengthOfValue(int value) => value == 0 ? 0 : (int)Math.Ceiling(Math.Log(value + 1, 32));

    /// <summary>Cryptographically random code of the given length, from the base32 alphabet.</summary>
    public static string RandomLongCode(int length = LongCodeLength) {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];
        for (var i = 0; i < length; i++) {
            chars[i] = Alphabet[bytes[i] & 0x1F];
        }
        return new string(chars);
    }

    private static void ThrowIfNegative(int value) {
        if (value < 0) {
            throw new ArgumentOutOfRangeException("", "Value must be non-negative.", nameof(value));
        }
    }
}
