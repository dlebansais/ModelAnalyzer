namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a literal value expression.
/// </summary>
internal class LiteralIntValueExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <summary>
    /// Gets or sets the literal value.
    /// </summary>
    public int Value { get; set; }

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
