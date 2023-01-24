namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents an unsupported invariant.
/// </summary>
internal class UnsupportedInvariant : IUnsupportedInvariant
{
    /// <inheritdoc/>
    required public string Text { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }
}
