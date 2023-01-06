namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents the type of an expression.
/// </summary>
[DebuggerDisplay("{Name}")]
public record ExpressionType
{
    /// <summary>
    /// Gets the 'Other' type.
    /// </summary>
    public static ExpressionType Other { get; } = new(string.Empty);

    /// <summary>
    /// Gets the 'Void' type.
    /// </summary>
    public static ExpressionType Void { get; } = new("void");

    /// <summary>
    /// Gets the 'Boolean' type.
    /// </summary>
    public static ExpressionType Boolean { get; } = new("bool");

    /// <summary>
    /// Gets the 'Integer' type.
    /// </summary>
    public static ExpressionType Integer { get; } = new("int");

    /// <summary>
    /// Gets the 'FloatingPoint' type.
    /// </summary>
    public static ExpressionType FloatingPoint { get; } = new("double");

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionType"/> class.
    /// </summary>
    /// <param name="name">the type name.</param>
    public ExpressionType(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the friendly type name.
    /// </summary>
    public string Name { get; }
}
