namespace Scampi.Domain.Extensions;

public static class StringExtensions
{
    public static string SanitizeString(this string arg)
    {
        return arg.Replace("\r", "").Replace("\n", "");
    }
}