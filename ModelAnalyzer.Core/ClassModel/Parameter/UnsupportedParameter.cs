namespace DemoAnalyzer;

using System.Diagnostics;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported parameter.
/// </summary>
[DebuggerDisplay("{ParameterName.Name}")]
internal class UnsupportedParameter : IUnsupportedParameter
{
    /// <inheritdoc/>
    public string Name => "*";

    /// <inheritdoc/>
    required public Location Location { get; init; }
}
