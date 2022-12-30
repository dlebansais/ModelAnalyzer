namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a local variable name.
/// </summary>
[DebuggerDisplay("{Text}")]
internal record LocalName : IClassMemberName, IVariableName
{
    /// <summary>
    /// Gets the local variable name.
    /// </summary>
    required public string Text { get; init; }
}
