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

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[""'\\(){}[\];:*~+\-!&|]")]
    private static partial Regex SpecialCharacterRegex();
}
