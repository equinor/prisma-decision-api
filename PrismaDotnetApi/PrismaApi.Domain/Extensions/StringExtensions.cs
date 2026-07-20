using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;

namespace PrismaApi.Domain.Extensions;

public static partial class StringExtensions
{
    public static string SanitizeLogString(this string arg)
    {
        return arg.Replace("\r", "").Replace("\n", "");
    }

    public static string SanitizeQuery(this string query)
    {
        var sanitized = query.Trim();
        sanitized = WhitespaceRegex().Replace(sanitized, " ");
        sanitized = SpecialCharacterRegex().Replace(sanitized, string.Empty);
        return sanitized;
    }

    public static Guid GenerateDeterministicGuid(this string input)
    {
        byte[] hashBytes = MD5.HashData(
            Encoding.UTF8.GetBytes(input)
        );
        return new Guid(hashBytes);
    }

    public static Guid GenerateDeterministicUuid(this string input)
    {
        byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));

        // RFC 4122 version 3 (MD5): set version nibble to 0x3 in byte 6
        hashBytes[6] = (byte)((hashBytes[6] & 0x0F) | 0x30);
        // RFC 4122 variant: set high bits of byte 8 to 10xxxxxx
        hashBytes[8] = (byte)((hashBytes[8] & 0x3F) | 0x80);

        // .NET's Guid(byte[]) reads bytes 0-3 as little-endian int32,
        // bytes 4-5 as little-endian int16, bytes 6-7 as little-endian int16.
        // Reverse those groups so the UUID string matches big-endian byte order.
        Array.Reverse(hashBytes, 0, 4);
        Array.Reverse(hashBytes, 4, 2);
        Array.Reverse(hashBytes, 6, 2);

        return new Guid(hashBytes);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[""'\\(){}[\];:*~+\-!&|]")]
    private static partial Regex SpecialCharacterRegex();
}
