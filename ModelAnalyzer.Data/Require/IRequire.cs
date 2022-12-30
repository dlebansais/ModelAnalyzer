namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about a requirement assertion.
/// </summary>
public interface IRequire
{
    /// <summary>
    /// Gets the assertion text.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Gets the assertion location.
    /// </summary>
    Location Location { get; }
}
