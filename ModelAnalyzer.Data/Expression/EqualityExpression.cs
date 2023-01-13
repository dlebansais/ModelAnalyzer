namespace ModelAnalyzer;

using System.Diagnostics;
using Newtonsoft.Json;

/// <summary>
/// Represents an equality expression.
/// </summary>
internal class EqualityExpression : Expression, IBinaryExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => false;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType(IMemberCollectionContext memberCollectionContext) => ExpressionType.Boolean;

    /// <summary>
    /// Gets the left expression.
    /// </summary>
    required public Expression Left { get; init; }

    /// <summary>
    /// Gets the equality operator.
    /// </summary>
    required public EqualityOperator Operator { get; init; }

    /// <summary>
    /// Gets the right expression.
    /// </summary>
    required public Expression Right { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string LeftString = Left.IsSimple ? $"{Left}" : $"({Left})";
        string RightString = Right.IsSimple ? $"{Right}" : $"({Right})";
        return $"{LeftString} {OperatorText.Equality[Operator]} {RightString}";
    }
}
