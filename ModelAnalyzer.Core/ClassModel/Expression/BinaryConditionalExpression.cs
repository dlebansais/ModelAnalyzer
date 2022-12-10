namespace ModelAnalyzer;

/// <summary>
/// Represents a binary conditional expression.
/// </summary>
internal class BinaryConditionalExpression : Expression
{
    /// <inheritdoc/>
    public override bool IsSimple => false;

    /// <summary>
    /// Gets the left expression.
    /// </summary>
    required public Expression Left { get; init; }

    /// <summary>
    /// Gets the logical operator.
    /// </summary>
    required public LogicalOperator Operator { get; init; }

    /// <summary>
    /// Gets the right expression.
    /// </summary>
    required public Expression Right { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string LeftString = Left.IsSimple ? $"{Left}" : $"({Left})";
        string RightString = Right.IsSimple ? $"{Right}" : $"({Right})";
        return $"{LeftString} {Operator.Text} {RightString}";
    }

    /// <inheritdoc/>
    public override string ToSimpleString()
    {
        return $"{Left} {Operator.Text} {Right}";
    }
}
