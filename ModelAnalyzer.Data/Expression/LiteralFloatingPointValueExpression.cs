﻿namespace ModelAnalyzer;

using System.Globalization;
using Newtonsoft.Json;

/// <summary>
/// Represents a literal floating point value expression.
/// </summary>
internal class LiteralFloatingPointValueExpression : Expression, ILiteralExpression<double>, ILiteralExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType() => ExpressionType.FloatingPoint;

    /// <inheritdoc/>
    public override LocationId LocationId { get; set; } = LocationId.CreateNew();

    /// <inheritdoc/>
    public double Value { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}
