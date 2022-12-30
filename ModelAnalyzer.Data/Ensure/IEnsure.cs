namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about a guarantee assertion.
/// </summary>
public interface IEnsure
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
