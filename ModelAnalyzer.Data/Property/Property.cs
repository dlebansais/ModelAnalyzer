namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a class property.
/// </summary>
[DebuggerDisplay("{Name.Text}")]
internal class Property : IProperty, INameable<PropertyName>
{
    /// <summary>
    /// Gets the Length property of arrays.
    /// </summary>
    public static Property ArrayLengthProperty { get; } = new Property() { Name = new PropertyName() { Text = "Length" }, Type = ExpressionType.Integer, Initializer = null };

    /// <summary>
    /// Gets the property name.
    /// </summary>
    required public PropertyName Name { get; init; }

    /// <inheritdoc/>
    IVariableName IVariable.Name { get => Name; }

    /// <inheritdoc/>
    required public ExpressionType Type { get; init; }

    /// <inheritdoc/>
    required public ILiteralExpression? Initializer { get; init; }
}
