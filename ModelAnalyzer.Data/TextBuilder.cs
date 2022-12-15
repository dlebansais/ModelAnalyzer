namespace ModelAnalyzer;

using System;

/// <summary>
/// Provides tools to normalize the new line sequence in logs and debug strings.
/// </summary>
public static class TextBuilder
{
    /// <summary>
    /// Gets or sets the new line sequence to use in debug strings.
    /// </summary>
    public static string NewLine { get; set; } = Environment.NewLine;

    /// <summary>
    /// Returns the text with normalized new line according to <see cref="NewLine"/>.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    public static string Normalized(string text)
    {
        string Result = text;

        Result = Result.Replace("\r\n", "\n");
        Result = Result.Replace("\r", "\n");
        Result = Result.Replace("\n", NewLine);

        return Result;
    }
}
