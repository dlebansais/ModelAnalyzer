namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a literal boolean value expression.
/// </summary>
internal class LiteralBoolValueExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <summary>
    /// Gets or sets a value indicating whether the literal value is true.
    /// </summary>
    public bool Value { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Value}";
    }
}
