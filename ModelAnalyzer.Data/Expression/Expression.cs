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
    /// <param name="parameterTable">The table of parameters.</param>
    /// <param name="resultField">The optional result field.</param>
    public abstract ExpressionType GetExpressionType(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Field? resultField);
}
