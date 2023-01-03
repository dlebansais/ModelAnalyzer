namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a property name.
/// </summary>
[DebuggerDisplay("{Text}")]
internal record PropertyName : IClassMemberName, IVariableName
{
    /// <summary>
    /// Gets the property name.
    /// </summary>
    required public string Text { get; init; }
}
