namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a unary conditional expression.
/// </summary>
internal class UnaryLogicalExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => false;

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
        string OperandString = Operand.IsSimple ? $"{Operand}" : $"({Operand.ToSimpleString()})";
        return $"{OperatorText.UnaryLogical[Operator]} {OperandString}";
    }

    /// <inheritdoc/>
    public override string ToSimpleString()
    {
        return $"{OperatorText.UnaryLogical[Operator]} {Operand}";
    }
}
