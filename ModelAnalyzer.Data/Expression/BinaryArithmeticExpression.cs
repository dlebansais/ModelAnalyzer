namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a binary arithmetic expression.
/// </summary>
internal class BinaryArithmeticExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => false;

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
        string LeftString = Left.IsSimple ? $"{Left}" : $"({Left.ToSimpleString()})";
        string RightString = Right.IsSimple ? $"{Right}" : $"({Right.ToSimpleString()})";
        return $"{LeftString} {OperatorText.BinaryArithmetic[Operator]} {RightString}";
    }

    /// <inheritdoc/>
    public override string ToSimpleString()
    {
        return ToString();
    }
}
