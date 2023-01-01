namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported expression.
/// </summary>
public class UnsupportedExpression : IUnsupportedExpression
{
    /// <inheritdoc/>
    required public Location Location { get; init; }
}
