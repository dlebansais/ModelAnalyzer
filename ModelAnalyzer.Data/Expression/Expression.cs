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
    /// <param name="fieldTable">The table of fields.</param>
    /// <param name="hostMethod">The host method, null in invariants.</param>
    /// <param name="resultLocal">The optional result local.</param>
    public abstract ExpressionType GetExpressionType(ReadOnlyFieldTable fieldTable, Method? hostMethod, Local? resultLocal);
}
