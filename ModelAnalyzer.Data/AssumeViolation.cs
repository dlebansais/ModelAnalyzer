namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a flow check violation.
/// </summary>
internal class AssumeViolation : IAssumeViolation
{
    /// <inheritdoc/>
    required public IMethod? Method { get; init; }

    /// <inheritdoc/>
    required public string Text { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }
}
