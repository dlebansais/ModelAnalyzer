namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported requirement.
/// </summary>
internal class UnsupportedRequire : IUnsupportedRequire
{
    /// <inheritdoc/>
    required public string Text { get; init; }

    /// <inheritdoc/>
    required public Location Location { get; init; }
}
