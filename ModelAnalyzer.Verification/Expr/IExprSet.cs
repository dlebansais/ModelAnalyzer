namespace ModelAnalyzer;

/// <summary>
/// Provides information about a set of Z3 expressions.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type.</typeparam>
internal interface IExprSet<out T> : IExprBase<T, T>
    where T : IExprCapsule
{
    /// <summary>
    /// Gets the list of all expressions in the set.
    /// </summary>
    IReadOnlyExprCollection<T> AllExpressions { get; }
}
