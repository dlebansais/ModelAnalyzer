namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a unary arithmetic expression.
/// </summary>
internal class UnaryArithmeticExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => Operand is LiteralIntegerValueExpression || Operand is LiteralFloatingPointValueExpression;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Field? resultField) => Operand.GetExpressionType(fieldTable, parameterTable, resultField);

    /// <summary>
    /// Gets the unary arithmetic operator.
    /// </summary>
    required public UnaryArithmeticOperator Operator { get; init; }

    /// <summary>
    /// Gets the operand expression.
    /// </summary>
    required public Expression Operand { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string OperandString = Operand.IsSimple ? $"{Operand}" : $"({Operand})";
        return $"{OperatorText.UnaryArithmetic[Operator]}{OperandString}";
    }
}
