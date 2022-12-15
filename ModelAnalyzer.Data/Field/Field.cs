namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a class field.
/// </summary>
[DebuggerDisplay("{FieldName.Name}")]
internal class Field : IField
{
    /// <summary>
    /// Gets the field name.
    /// </summary>
    required public FieldName FieldName { get; init; }

    /// <inheritdoc/>
    public string Name { get { return FieldName.Name; } }
}
