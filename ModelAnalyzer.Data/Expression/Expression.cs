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
    /// Returns a string that represents the current object, with no parenthesis around.
    /// </summary>
    public abstract string ToSimpleString();
}
