namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported method.
/// </summary>
internal class UnsupportedMethod : IUnsupportedMethod
{
    /// <inheritdoc/>
    public string Name => "*";

    /// <inheritdoc/>
    required public Location Location { get; init; }
}
