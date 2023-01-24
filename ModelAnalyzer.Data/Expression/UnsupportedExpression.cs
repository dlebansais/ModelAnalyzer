namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents an unsupported expression.
/// </summary>
internal class UnsupportedExpression : IUnsupportedExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }
}
