namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a guarantee that is not supported.
/// </summary>
internal class UnsupportedEnsure : IUnsupportedEnsure
{
    /// <inheritdoc/>
    required public string Text { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }
}
