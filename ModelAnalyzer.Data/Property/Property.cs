namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a class property.
/// </summary>
[DebuggerDisplay("{Name.Text}")]
internal record Property : Variable, IProperty, INameable<PropertyName>
{
    /// <summary>
    /// Gets the Length property of arrays.
    /// </summary>
    public static Property ArrayLengthProperty { get; } = new Property(new PropertyName() { Text = "Length" }, ExpressionType.Integer)
    {
        Initializer = null,
        ClassName = new ClassName()
        {
            Namespace = new List<string>() { "System" },
            Text = "Array",
        },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Property"/> class.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="type">The property type.</param>
    public Property(PropertyName name, ExpressionType type)
        : base(name, type)
    {
    }

    /// <inheritdoc/>
    IVariableName IVariable.Name { get => Name; }

    /// <inheritdoc/>
    PropertyName INameable<PropertyName>.Name { get => (PropertyName)Name; }

    /// <inheritdoc/>
    required public ILiteralExpression? Initializer { get; init; }

    /// <inheritdoc/>
    required public ClassName ClassName { get; init; }
}
