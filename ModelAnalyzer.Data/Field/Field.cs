namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a class field.
/// </summary>
[DebuggerDisplay("{Name.Text}")]
internal class Field : IField, INameable<FieldName>
{
    /// <summary>
    /// Gets the field name.
    /// </summary>
    required public FieldName Name { get; init; }

    /// <inheritdoc/>
    IVariableName IVariable.Name { get => Name; }

    /// <inheritdoc/>
    required public ExpressionType Type { get; init; }

    /// <inheritdoc/>
    required public ILiteralExpression? Initializer { get; init; }

    /// <inheritdoc/>
    required public ClassName ClassName { get; init; }
}
