namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a integer division remainder expression.
/// </summary>
internal class RemainderExpression : Expression, IBinaryExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => false;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType(IMemberCollectionContext memberCollectionContext) => ExpressionType.Integer;

    /// <summary>
    /// Gets the left expression.
    /// </summary>
    required public Expression Left { get; init; }

    /// <summary>
    /// Gets the right expression.
    /// </summary>
    required public Expression Right { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string LeftString = Left.IsSimple ? $"{Left}" : $"({Left})";
        string RightString = Right.IsSimple ? $"{Right}" : $"({Right})";
        return $"{LeftString} % {RightString}";
    }
}
