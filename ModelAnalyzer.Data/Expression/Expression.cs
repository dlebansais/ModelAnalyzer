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
    /// <param name="memberCollectionContext">The context.</param>
    public abstract ExpressionType GetExpressionType(IMemberCollectionContext memberCollectionContext);
}
