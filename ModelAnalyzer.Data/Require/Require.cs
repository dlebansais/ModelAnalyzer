namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a requirement assertion.
/// </summary>
internal class Require : IRequire
{
    /// <inheritdoc/>
    [JsonIgnore]
    required public string Text { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }

    /// <summary>
    /// Gets the expression representing the requirement.
    /// </summary>
    required public Expression BooleanExpression { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}
