namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

/// <summary>
/// Represents the type of an expression.
/// </summary>
[DebuggerDisplay("\"{Name, nq}{(IsNullable ? \"?\" : \"\"), nq}\"")]
public record ExpressionType
{
    /// <summary>
    /// Gets the 'Other' type.
    /// </summary>
    public static ExpressionType Other { get; } = new(string.Empty, isNullable: false);

    /// <summary>
    /// Gets the 'Void' type.
    /// </summary>
    public static ExpressionType Void { get; } = new("void", isNullable: false);

    /// <summary>
    /// Gets the 'Boolean' type.
    /// </summary>
    public static ExpressionType Boolean { get; } = new("bool", isNullable: false);

    /// <summary>
    /// Gets the 'Integer' type.
    /// </summary>
    public static ExpressionType Integer { get; } = new("int", isNullable: false);

    /// <summary>
    /// Gets the 'FloatingPoint' type.
    /// </summary>
    public static ExpressionType FloatingPoint { get; } = new("double", isNullable: false);

    /// <summary>
    /// Gets the 'Null' type.
    /// </summary>
    public static ExpressionType Null { get; } = new(string.Empty, isNullable: true);

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionType"/> class.
    /// </summary>
    /// <param name="name">The type name.</param>
    /// <param name="isNullable">Whether the type can have value 'null'.</param>
    public ExpressionType(string name, bool isNullable)
    {
        Name = name;
        IsNullable = isNullable;
    }

    /// <summary>
    /// Gets the friendly type name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the type can have value 'null'.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// Gets a value indicating whether the type is one of the simple predefined types.
    /// </summary>
    public bool IsSimple
    {
        get
        {
            Debug.Assert(this != Other);

            List<ExpressionType> SimpleExpressionTypeList = new()
            {
                Void,
                Boolean,
                Integer,
                FloatingPoint,
            };

            return SimpleExpressionTypeList.Contains(this);
        }
    }
}
