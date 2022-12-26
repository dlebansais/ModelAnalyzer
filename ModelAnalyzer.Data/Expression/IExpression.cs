namespace ModelAnalyzer;

/// <summary>
/// Provides information about an expression.
/// </summary>
public interface IExpression
{
    /// <summary>
    /// Gets the expression type.
    /// </summary>
    ExpressionType ExpressionType { get; }
}
