namespace ModelAnalyzer;

using System.Diagnostics;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported parameter.
/// </summary>
[DebuggerDisplay("{ParameterName.Name}")]
public class UnsupportedParameter : IUnsupportedParameter
{
    /// <inheritdoc/>
    public string Name => "*";

    /// <inheritdoc/>
    required public Location Location { get; init; }
}
