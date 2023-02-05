namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents a set of Z3 expressions.
/// </summary>
internal class ExprObject : IExprSet<IRefExprCapsule, IExprCapsule>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExprObject"/> class.
    /// </summary>
    /// <param name="expr">The expression for the reference that makes the set.</param>
    /// <param name="propertyExpressions">The expressions for properties.</param>
    public ExprObject(IRefExprCapsule expr, ICollection<IExprSet<IExprCapsule, IExprCapsule>> propertyExpressions)
    {
        ExprCollection<IExprCapsule> ExpressionList = new() { expr };
        ExprCollection<IExprCapsule> OtherExpressionList = new();

        foreach (IExprSet<IExprCapsule, IExprCapsule> Item in propertyExpressions)
        {
            ExpressionList.AddRange(Item.AllExpressions);
            OtherExpressionList.AddRange(Item.AllExpressions);
        }

        AllExpressions = ExpressionList.AsReadOnly();
        OtherExpressions = OtherExpressionList.AsReadOnly();
    }

    /// <inheritdoc/>
    public IRefExprCapsule MainExpression { get => (IRefExprCapsule)AllExpressions[0]; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<IExprCapsule> OtherExpressions { get; }

    /// <inheritdoc/>
    public bool IsSingle { get => OtherExpressions.Count == 0; }

    /// <inheritdoc/>
    public IReadOnlyExprCollection<IExprCapsule> AllExpressions { get; }
}
