namespace ModelAnalyzer;

/// <summary>
/// Represents a single Z3 expression.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type.</typeparam>
internal class ExprSingle<T> : IExprSingle<T>
    where T : IExprCapsule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExprSingle{T}"/> class.
    /// </summary>
    /// <param name="expr">The expression that makes the set.</param>
    public ExprSingle(T expr)
    {
        MainExpression = expr;
        OtherExpressions = new ExprCollection<T>().AsReadOnly();
        AllExpressions = new ExprCollection<T>() { expr }.AsReadOnly();
    }

    /// <inheritdoc/>
    public T MainExpression { get; init; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<T> OtherExpressions { get; }

    /// <inheritdoc/>
    public bool IsSingle { get => OtherExpressions.Count == 0; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<T> AllExpressions { get; }
}
