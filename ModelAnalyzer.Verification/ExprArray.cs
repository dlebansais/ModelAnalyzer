namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents a set of Z3 expressions.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type for element expressions.</typeparam>
internal class ExprArray<T> : IExprBase<IRefExprCapsule, T>
    where T : IExprCapsule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExprArray{T}"/> class.
    /// </summary>
    /// <param name="expr">The expression for the reference that makes the set.</param>
    /// <param name="elementExpressions">The expressions for elements.</param>
    public ExprArray(IRefExprCapsule expr, List<T> elementExpressions)
    {
        MainExpression = expr;
        OtherExpressions = new ExprCollection<T>(elementExpressions).AsReadOnly();
    }

    /// <inheritdoc/>
    public IRefExprCapsule MainExpression { get; init; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<T> OtherExpressions { get; }

    /// <inheritdoc/>
    public bool IsSingle { get => OtherExpressions.Count == 0; }

    /// <summary>
    /// Converts to a set of expressions.
    /// </summary>
    public IExprSet<IExprCapsule> ToExprSet()
    {
        List<IExprCapsule> ExpressionList = new() { MainExpression };

        foreach (T Item in OtherExpressions)
            ExpressionList.Add(Item);

        return new ExprSet<IExprCapsule>(ExpressionList);
    }
}
