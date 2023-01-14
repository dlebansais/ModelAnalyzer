namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides information about a read-only collection of Z3 expressions.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type.</typeparam>
internal interface IReadOnlyExprCollection<out T> : IReadOnlyList<T>
    where T : IExprCapsule
{
}
