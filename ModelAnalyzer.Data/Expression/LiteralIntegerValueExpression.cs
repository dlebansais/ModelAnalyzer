namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a literal integer value expression.
/// </summary>
internal class LiteralIntegerValueExpression : Expression, ILiteralExpression<int>, ILiteralExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    [JsonIgnore]
    public override ExpressionType ExpressionType => ExpressionType.Integer;

    /// <inheritdoc/>
    public int Value { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value.ToString();
    }
}
