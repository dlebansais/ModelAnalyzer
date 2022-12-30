namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about a class invariant assertion.
/// </summary>
public interface IInvariant
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
