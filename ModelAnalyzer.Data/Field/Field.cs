namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a class field.
/// </summary>
[DebuggerDisplay("{Name.Text}")]
internal class Field : CodeVariable, IField, INameable<FieldName>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Field"/> class.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="type">The field type.</param>
    public Field(FieldName name, ExpressionType type)
        : base(name, type)
    {
    }

    /// <inheritdoc/>
    IVariableName IVariable.Name { get => Name; }

    /// <inheritdoc/>
    FieldName INameable<FieldName>.Name { get => (FieldName)Name; }

    /// <inheritdoc/>
    required public ILiteralExpression? Initializer { get; init; }

    /// <inheritdoc/>
    required public ClassName ClassName { get; init; }
}
