namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a parameter name.
/// </summary>
[DebuggerDisplay("{Text}")]
internal record ParameterName : IVariableName
{
    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    required public string Text { get; init; }
}
