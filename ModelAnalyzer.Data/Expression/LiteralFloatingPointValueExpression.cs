namespace ModelAnalyzer;

using System.Globalization;
using Newtonsoft.Json;

/// <summary>
/// Represents a literal floating point value expression.
/// </summary>
internal class LiteralFloatingPointValueExpression : Expression, ILiteralExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    [JsonIgnore]
    public override ExpressionType ExpressionType => ExpressionType.FloatingPoint;

    /// <summary>
    /// Gets or sets the literal value.
    /// </summary>
    public double Value { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}
