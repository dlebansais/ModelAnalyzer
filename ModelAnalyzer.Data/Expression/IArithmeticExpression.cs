namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an arithmetic expression.
/// </summary>
public interface IArithmeticExpression
{
    /// <summary>
    /// Gets the expression location.
    /// </summary>
    Location Location { get; }
}
