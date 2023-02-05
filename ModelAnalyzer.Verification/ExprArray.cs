namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a set of Z3 expressions.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type for element expressions.</typeparam>
internal class ExprArray<T> : IExprSet<IRefExprCapsule, T>
    where T : IExprCapsule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExprArray{T}"/> class.
    /// </summary>
    /// <param name="expr">The expression for the reference that makes the set.</param>
    /// <param name="elementExpressions">The expressions for elements.</param>
    public ExprArray(IRefExprCapsule expr, List<T> elementExpressions)
    {
        List<IExprCapsule> ExpressionList = new() { expr };
        List<T> OtherExpressionList = new();

        for (int i = 0; i < elementExpressions.Count; i++)
        {
            T Item = elementExpressions[i];
            ExpressionList.Add(Item);
            OtherExpressionList.Add(Item);
        }

        AllExpressions = new ExprCollection<IExprCapsule>(ExpressionList).AsReadOnly();
        OtherExpressions = new ExprCollection<T>(OtherExpressionList).AsReadOnly();
    }

    /// <inheritdoc/>
    public IRefExprCapsule MainExpression { get => (IRefExprCapsule)AllExpressions[0]; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<T> OtherExpressions { get; }

    /// <inheritdoc/>
    public bool IsSingle { get => OtherExpressions.Count == 0; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<IExprCapsule> AllExpressions { get; }
}
