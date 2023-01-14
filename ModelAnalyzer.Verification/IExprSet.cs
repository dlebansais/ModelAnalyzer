namespace ModelAnalyzer;

/// <summary>
/// Represents a set of Z3 expressions.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type.</typeparam>
internal interface IExprSet<out T>
    where T : IExprCapsule
{
    /// <summary>
    /// Gets the main expression in the set.
    /// </summary>
    T MainExpression { get; }

    /// <summary>
    /// Gets a value indicating whether the set contains exactly one expression.
    /// </summary>
    bool IsSingle { get; }

    /// <summary>
    /// Gets the list of expressions in the set.
    /// </summary>
    IReadOnlyExprCollection<T> Expressions { get; }
}
