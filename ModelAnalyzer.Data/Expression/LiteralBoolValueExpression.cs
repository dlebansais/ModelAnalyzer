namespace ModelAnalyzer;

/// <summary>
/// Represents a literal boolean value expression.
/// </summary>
internal class LiteralBoolValueExpression : Expression
{
    /// <inheritdoc/>
    public override bool IsSimple => true;

    /// <summary>
    /// Gets a value indicating whether the literal value is true.
    /// </summary>
    required public bool Value { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Value}";
    }

    /// <inheritdoc/>
    public override string ToSimpleString()
    {
        return ToString();
    }
}
