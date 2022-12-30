namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a guarantee when a method returns.
/// </summary>
internal class Ensure : IEnsure, IAssertion
{
    /// <summary>
    /// The result keyword for ensure assertions.
    /// </summary>
    public const string ResultKeyword = "Result";

    /// <inheritdoc/>
    [JsonIgnore]
    required public string Text { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }

    /// <inheritdoc/>
    required public IExpression BooleanExpression { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}
