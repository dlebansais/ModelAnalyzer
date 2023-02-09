namespace ModelAnalyzer;

/// <summary>
/// Represents a set of Z3 expressions.
/// </summary>
/// <typeparam name="TMain">The specialized Z3 expression type for the main expression.</typeparam>
/// <typeparam name="TOther">The specialized Z3 expression type for other expressions.</typeparam>
internal interface IExprBase<out TMain, out TOther>
    where TMain : IExprCapsule
    where TOther : IExprCapsule
{
    /// <summary>
    /// Gets the main expression in the set.
    /// </summary>
    TMain MainExpression { get; }

    /// <summary>
    /// Gets the list of other expressions in the set.
    /// </summary>
    IReadOnlyExprCollection<TOther> OtherExpressions { get; }
}
