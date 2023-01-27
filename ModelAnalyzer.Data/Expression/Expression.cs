namespace ModelAnalyzer;

/// <summary>
/// Represents an expression.
/// </summary>
internal abstract class Expression : IExpression
{
    /// <summary>
    /// Gets a value indicating whether the expression is simple.
    /// </summary>
    public abstract bool IsSimple { get; }

    /// <summary>
    /// Gets the expression type.
    /// </summary>
    public abstract ExpressionType GetExpressionType();

    /// <summary>
    /// Gets or sets the location Id.
    /// </summary>
    public abstract LocationId LocationId { get; set; }
}
