namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a literal boolean value expression.
/// </summary>
internal class LiteralBooleanValueExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    [JsonIgnore]
    public override ExpressionType ExpressionType => ExpressionType.Boolean;

    /// <summary>
    /// Gets or sets a value indicating whether the literal value is true.
    /// </summary>
    public bool Value { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value ? "true" : "false";
    }
}
