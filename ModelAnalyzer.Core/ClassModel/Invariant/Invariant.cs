namespace ModelAnalyzer;

/// <summary>
/// Represents a class invariant.
/// </summary>
internal class Invariant : IInvariant
{
    /// <inheritdoc/>
    required public string Text { get; init; }

    /// <summary>
    /// Gets the invariant expression.
    /// </summary>
    required public IExpression BooleanExpression { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}
