namespace ModelAnalyzer;

/// <summary>
/// Provides information about a set of Z3 expressions.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type.</typeparam>
internal interface IExprSingle<out T> : IExprSet<T>
    where T : IExprCapsule
{
}
