namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides information about a collection of Z3 expressions.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type.</typeparam>
internal interface IExprCollection<T> : ICollection<T>
    where T : IExprCapsule
{
}
