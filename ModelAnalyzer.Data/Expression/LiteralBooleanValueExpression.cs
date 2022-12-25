namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a literal boolean value expression.
/// </summary>
internal class LiteralBooleanValueExpression : Expression, ILiteralExpression<bool>, ILiteralExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    [JsonIgnore]
    public override ExpressionType ExpressionType => ExpressionType.Boolean;

    /// <inheritdoc/>
    public bool Value { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value ? "true" : "false";
    }
}
