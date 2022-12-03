namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported expression.
/// </summary>
internal class UnsupportedExpression : IUnsupportedExpression
{
    /// <inheritdoc/>
    required public Location Location { get; init; }
}
