namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents an unsupported requirement.
/// </summary>
internal class UnsupportedRequire : IUnsupportedRequire
{
    /// <inheritdoc/>
    required public string Text { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }
}
