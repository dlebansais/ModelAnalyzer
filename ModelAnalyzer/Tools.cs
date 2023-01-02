namespace ModelAnalyzer;

public static class Tools
{
    public static string Truncate(this string value, int maxChars = 40)
    {
        return value.Length <= maxChars ? value : value.Substring(0, maxChars) + (char)0x2026;
    }
}
