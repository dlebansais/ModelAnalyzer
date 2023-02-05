namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents a set of Z3 expressions.
/// </summary>
internal class ExprObject : IExprBase<IRefExprCapsule, IExprCapsule>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExprObject"/> class.
    /// </summary>
    /// <param name="expr">The expression for the reference that makes the set.</param>
    /// <param name="propertyExpressions">The expressions for properties.</param>
    public ExprObject(IRefExprCapsule expr, ICollection<IExprBase<IExprCapsule, IExprCapsule>> propertyExpressions)
    {
        MainExpression = expr;

        ExprCollection<IExprCapsule> OtherExpressionList = new();

        foreach (IExprBase<IExprCapsule, IExprCapsule> Item in propertyExpressions)
        {
            OtherExpressionList.Add(Item.MainExpression);
            OtherExpressionList.AddRange(Item.OtherExpressions);
        }

        OtherExpressions = OtherExpressionList.AsReadOnly();
    }

    /// <inheritdoc/>
    public IRefExprCapsule MainExpression { get; init; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<IExprCapsule> OtherExpressions { get; }

    /// <inheritdoc/>
    public bool IsSingle { get; } = false;

    /// <summary>
    /// Converts to a set of expressions.
    /// </summary>
    public IExprSet<IExprCapsule> ToExprSet()
    {
        List<IExprCapsule> ExpressionList = new() { MainExpression };
        ExpressionList.AddRange(OtherExpressions);

        return new ExprSet<IExprCapsule>(ExpressionList);
    }
}
