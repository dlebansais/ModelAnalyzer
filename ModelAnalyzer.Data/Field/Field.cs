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
    public IVariableName VariableName { get => FieldName; }

    /// <inheritdoc/>
    required public ExpressionType VariableType { get; init; }

    /// <summary>
    /// Gets the field initializer.
    /// </summary>
    required public ILiteralExpression? Initializer { get; init; }
}
