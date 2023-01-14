namespace ModelAnalyzer;

using System.Collections.Generic;

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
    /// <param name="expr">The name that makes the set.</param>
    public ExprSet(T expr)
    {
        ExprCollection<T> ExpressionList = new() { expr };
        Expressions = ExpressionList.AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExprSet{T}"/> class.
    /// </summary>
    /// <param name="expressionSets">The list of expressions.</param>
    public ExprSet(ICollection<IExprSet<T>> expressionSets)
    {
        List<T> ExpressionList = new();

        foreach (IExprSet<T> Item in expressionSets)
            ExpressionList.AddRange(Item.Expressions);

        Expressions = new ExprCollection<T>(ExpressionList).AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExprSet{T}"/> class.
    /// </summary>
    /// <param name="expr">The expression for the reference that makes the set.</param>
    /// <param name="propertyExpressions">The expressions for properties.</param>
    public ExprSet(T expr, ICollection<IExprSet<T>> propertyExpressions)
    {
        ExprCollection<T> ExpressionList = new() { expr };

        foreach (IExprSet<T> Item in propertyExpressions)
            ExpressionList.AddRange(Item.Expressions);

        Expressions = ExpressionList.AsReadOnly();
    }

    /// <inheritdoc/>
    public T MainExpression { get => Expressions[0]; }

    /// <inheritdoc/>
    public bool IsSingle { get => Expressions.Count == 1; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<T> Expressions { get; }
}
