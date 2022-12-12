namespace ModelAnalyzer;

/// <summary>
/// Represents a unary arithmetic expression.
/// </summary>
internal class UnaryArithmeticExpression : Expression
{
    /// <inheritdoc/>
    public override bool IsSimple => false;

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
        string OperandString = Operand.IsSimple ? $"{Operand}" : $"({Operand.ToSimpleString()})";
        return $"{Operator.Text}{OperandString}";
    }

    /// <inheritdoc/>
    public override string ToSimpleString()
    {
        return $"{Operator.Text}{Operand}";
    }
}
