namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a field name.
/// </summary>
[DebuggerDisplay("{Text}")]
internal record FieldName : IClassMemberName, IVariableName
{
    /// <summary>
    /// Gets the field name.
    /// </summary>
    required public string Text { get; init; }
}
