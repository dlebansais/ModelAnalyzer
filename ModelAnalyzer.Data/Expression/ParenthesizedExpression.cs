namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a parenthesized expression.
/// </summary>
internal class ParenthesizedExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => false;

    /// <summary>
    /// Gets the expression within parenthesis.
    /// </summary>
    required public Expression Inside { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Inside.ToString();
    }
}
