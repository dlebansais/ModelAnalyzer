namespace DemoAnalyzer;

/// <summary>
/// Provides information about an expression.
/// </summary>
public interface IExpression
{
    /// <summary>
    /// Gets a value indicating whether the expression is simple.
    /// </summary>
    bool IsSimple { get; }
}
