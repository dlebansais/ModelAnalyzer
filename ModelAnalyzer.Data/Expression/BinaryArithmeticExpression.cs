namespace ModelAnalyzer;

using System.Diagnostics;
using Newtonsoft.Json;

/// <summary>
/// Represents a binary arithmetic expression.
/// </summary>
internal class BinaryArithmeticExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => false;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType(IMemberCollectionContext memberCollectionContext)
    {
        ExpressionType LeftExpressionType = Left.GetExpressionType(memberCollectionContext);
        ExpressionType RightExpressionType = Right.GetExpressionType(memberCollectionContext);

        if (LeftExpressionType == ExpressionType.FloatingPoint || RightExpressionType == ExpressionType.FloatingPoint)
            return ExpressionType.FloatingPoint;

        Debug.Assert(LeftExpressionType == ExpressionType.Integer);
        Debug.Assert(RightExpressionType == ExpressionType.Integer);

        return ExpressionType.Integer;
    }

    /// <summary>
    /// Gets the left expression.
    /// </summary>
    required public Expression Left { get; init; }

    /// <summary>
    /// Gets the binary arithmetic operator.
    /// </summary>
    required public BinaryArithmeticOperator Operator { get; init; }

    /// <summary>
    /// Gets the right expression.
    /// </summary>
    required public Expression Right { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string LeftString = Left.IsSimple ? $"{Left}" : $"({Left})";
        string RightString = Right.IsSimple ? $"{Right}" : $"({Right})";
        return $"{LeftString} {OperatorText.BinaryArithmetic[Operator]} {RightString}";
    }
}
