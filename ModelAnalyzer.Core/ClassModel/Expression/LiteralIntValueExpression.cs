namespace ModelAnalyzer;

/// <summary>
/// Represents a literal value expression.
/// </summary>
internal class LiteralIntValueExpression : Expression
{
    /// <inheritdoc/>
    public override bool IsSimple => true;

    /// <summary>
    /// Gets the literal value.
    /// </summary>
    required public int Value { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Value}";
    }
}
