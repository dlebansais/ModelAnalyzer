namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents an unsupported statement.
/// </summary>
internal class UnsupportedStatement : IUnsupportedStatement
{
    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }
}
