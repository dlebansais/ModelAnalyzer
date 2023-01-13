namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a unary conditional expression.
/// </summary>
internal class UnaryLogicalExpression : Expression, IUnaryExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => false;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType(IMemberCollectionContext memberCollectionContext) => ExpressionType.Boolean;

    /// <summary>
    /// Gets the logical operator.
    /// </summary>
    required public UnaryLogicalOperator Operator { get; init; }

    /// <summary>
    /// Gets the operand expression.
    /// </summary>
    required public Expression Operand { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string OperandString = Operand.IsSimple ? $"{Operand}" : $"({Operand})";
        return $"{OperatorText.UnaryLogical[Operator]}{OperandString}";
    }
}
