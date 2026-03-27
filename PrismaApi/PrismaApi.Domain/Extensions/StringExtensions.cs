using System.Text.RegularExpressions;

namespace Scampi.Domain.Extensions;

public static class StringExtensions
{
    public static string SanitizeLogString(this string arg)
    {
        return arg.Replace("\r", "").Replace("\n", "");
    }

    public static string SanitizeQuery(this string query)
    {
        var sanitized = query.Trim();
        sanitized = Regex.Replace(sanitized, @"\s+", " ");
        sanitized = Regex.Replace(sanitized, @"[""'\\(){}[\];:*~+\-!&|]", string.Empty);
        return sanitized;
    }
}
