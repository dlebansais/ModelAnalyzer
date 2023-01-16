namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents the literal 'null' expression.
/// </summary>
internal class LiteralNullExpression : Expression, ILiteralExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType() => ExpressionType.Null;

    /// <inheritdoc/>
    public override string ToString()
    {
        return "null";
    }
}
