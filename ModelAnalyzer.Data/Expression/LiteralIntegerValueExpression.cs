﻿namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a literal integer value expression.
/// </summary>
internal class LiteralIntegerValueExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    [JsonIgnore]
    public override ExpressionType ExpressionType => ExpressionType.Integer;

    /// <summary>
    /// Gets or sets the literal value.
    /// </summary>
    public int Value { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value.ToString();
    }
}