namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides information about a read-only collection of Z3 expressions.
/// </summary>
internal interface IReadOnlyExprCollection : IReadOnlyCollection<IExprCapsule>
{
}
