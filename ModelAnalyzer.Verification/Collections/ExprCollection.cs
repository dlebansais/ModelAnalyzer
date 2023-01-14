namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents a collection of Z3 expressions.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type.</typeparam>
internal class ExprCollection<T> : List<T>, IExprCollection<T>
    where T : IExprCapsule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExprCollection{T}"/> class.
    /// </summary>
    public ExprCollection()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExprCollection{T}"/> class.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    public ExprCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

    /// <summary>
    /// Returns a read-only <see cref="ReadOnlyExprCollection{T}"/> wrapper for the current collection.
    /// </summary>
    public new IReadOnlyExprCollection<T> AsReadOnly()
    {
        return new ReadOnlyExprCollection<T>(this);
    }
}
