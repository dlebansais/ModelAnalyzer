namespace ModelAnalyzer;

/// <summary>
/// Represents a set of Z3 expressions.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type for element expressions.</typeparam>
internal class ExprArray<T> : IExprBase<IRefExprCapsule, IExprCapsule>
    where T : IArrayExprCapsule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExprArray{T}"/> class.
    /// </summary>
    /// <param name="expr">The expression for the reference that makes the set.</param>
    /// <param name="elementExpression">The expressions for elements.</param>
    /// <param name="sizeExpr">The expressions for the array size.</param>
    public ExprArray(IRefExprCapsule expr, T elementExpression, IIntExprCapsule sizeExpr)
    {
        MainExpression = expr;

        ExprCollection<IExprCapsule> ExprList = new()
        {
            elementExpression,
            sizeExpr,
        };

        OtherExpressions = ExprList.AsReadOnly();
    }

    /// <inheritdoc/>
    public IRefExprCapsule MainExpression { get; init; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<IExprCapsule> OtherExpressions { get; }

    /// <inheritdoc/>
    public bool IsSingle { get; } = false;

    /// <summary>
    /// Gets the array expression.
    /// </summary>
    public T ArrayExpression { get => (T)OtherExpressions[0]; }

    /// <summary>
    /// Gets the size expression.
    /// </summary>
    public IIntExprCapsule SizeExpression { get => (IIntExprCapsule)OtherExpressions[1]; }
}
