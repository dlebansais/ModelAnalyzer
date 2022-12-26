namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a parameter name.
/// </summary>
[DebuggerDisplay("{Name}")]
internal record ParameterName : IVariableName
{
    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    required public string Name { get; init; }
}
