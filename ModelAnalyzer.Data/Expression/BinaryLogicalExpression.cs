﻿namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a binary conditional expression.
/// </summary>
internal class BinaryLogicalExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => false;

    /// <inheritdoc/>
    [JsonIgnore]
    public override ExpressionType ExpressionType => ExpressionType.Bool;

    /// <summary>
    /// Gets the left expression.
    /// </summary>
    required public Expression Left { get; init; }

    /// <summary>
    /// Gets the logical operator.
    /// </summary>
    required public BinaryLogicalOperator Operator { get; init; }

    /// <summary>
    /// Gets the right expression.
    /// </summary>
    required public Expression Right { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string LeftString = Left.IsSimple ? $"{Left}" : $"({Left})";
        string RightString = Right.IsSimple ? $"{Right}" : $"({Right})";
        return $"{LeftString} {OperatorText.BinaryLogical[Operator]} {RightString}";
    }
}
