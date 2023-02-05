namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a set of Z3 expressions.
/// </summary>
/// <typeparam name="T">The specialized Z3 expression type.</typeparam>
internal class ExprSet<T> : IExprSet<T>
    where T : IExprCapsule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExprSet{T}"/> class.
    /// </summary>
    /// <param name="expressions">The list of expressions.</param>
    public ExprSet(List<T> expressions)
    {
        Debug.Assert(expressions.Count > 0);

        List<T> ExpressionList = new() { expressions[0] };
        List<T> OtherExpressionList = new();

        for (int i = 1; i < expressions.Count; i++)
        {
            T Item = expressions[i];
            ExpressionList.Add(Item);
            OtherExpressionList.Add(Item);
        }

        AllExpressions = new ExprCollection<T>(ExpressionList).AsReadOnly();
        OtherExpressions = new ExprCollection<T>(OtherExpressionList).AsReadOnly();
    }

    /// <inheritdoc/>
    public T MainExpression { get => AllExpressions[0]; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<T> OtherExpressions { get; }

    /// <inheritdoc/>
    public bool IsSingle { get => OtherExpressions.Count == 0; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<T> AllExpressions { get; }
}
