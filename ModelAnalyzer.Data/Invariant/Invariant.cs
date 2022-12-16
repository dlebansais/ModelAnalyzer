namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a class invariant.
/// </summary>
internal class Invariant : IInvariant
{
    /// <inheritdoc/>
    [JsonIgnore]
    required public string Text { get; init; }

    /// <summary>
    /// Gets the invariant expression.
    /// </summary>
    required public Expression BooleanExpression { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}
