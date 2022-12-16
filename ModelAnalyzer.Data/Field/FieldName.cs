namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a field name.
/// </summary>
[DebuggerDisplay("{Name}")]
internal record FieldName : IClassMemberName
{
    /// <summary>
    /// Gets the field name.
    /// </summary>
    required public string Name { get; init; }
}
