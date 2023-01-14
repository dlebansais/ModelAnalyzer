namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Represents a collection of Z3 expressions.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type.</typeparam>
internal class ReadOnlyExprCollection<T> : ReadOnlyCollection<T>, IReadOnlyExprCollection<T>
    where T : IExprCapsule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyExprCollection{T}"/> class.
    /// </summary>
    /// <param name="list">The list of expressions.</param>
    public ReadOnlyExprCollection(IList<T> list)
        : base(list)
    {
    }
}
