namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an unsupported expression.
/// </summary>
public interface IUnsupportedExpression : IExpression
{
    /// <summary>
    /// Gets the expression location.
    /// </summary>
    Location Location { get; }
}
