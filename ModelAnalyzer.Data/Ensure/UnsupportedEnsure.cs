namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a guarantee that is not supported.
/// </summary>
internal class UnsupportedEnsure : IUnsupportedEnsure
{
    /// <inheritdoc/>
    required public string Text { get; init; }

    /// <inheritdoc/>
    required public Location Location { get; init; }
}
