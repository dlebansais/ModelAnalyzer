namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

/// <summary>
/// Represents the type of an expression.
/// </summary>
[DebuggerDisplay("{TypeName.ToString(), nq}{(IsNullable ? \"?\" : \"\"), nq}{(IsArray ? \"[]\" : \"\"), nq}")]
public record ExpressionType
{
    /// <summary>
    /// Gets the 'Other' type.
    /// </summary>
    public static ExpressionType Other { get; } = new(ClassName.Empty, isNullable: false, isArray: false);

    /// <summary>
    /// Gets the 'Void' type.
    /// </summary>
    public static ExpressionType Void { get; } = new(ClassName.FromSimpleString("void"), isNullable: false, isArray: false);

    /// <summary>
    /// Gets the 'Boolean' type.
    /// </summary>
    public static ExpressionType Boolean { get; } = new(ClassName.FromSimpleString("bool"), isNullable: false, isArray: false);

    /// <summary>
    /// Gets the 'Integer' type.
    /// </summary>
    public static ExpressionType Integer { get; } = new(ClassName.FromSimpleString("int"), isNullable: false, isArray: false);

    /// <summary>
    /// Gets the 'FloatingPoint' type.
    /// </summary>
    public static ExpressionType FloatingPoint { get; } = new(ClassName.FromSimpleString("double"), isNullable: false, isArray: false);

    /// <summary>
    /// Gets the 'Null' type.
    /// </summary>
    public static ExpressionType Null { get; } = new(ClassName.Empty, isNullable: true, isArray: false);

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionType"/> class.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <param name="isNullable">Whether the type can have value 'null'.</param>
    /// <param name="isArray">Whether the type is an array.</param>
    [JsonConstructor]
    public ExpressionType(ClassName typeName, bool isNullable, bool isArray)
    {
        TypeName = typeName;
        IsNullable = isNullable;
        IsArray = isArray;
    }

    /// <summary>
    /// Gets the friendly type name.
    /// </summary>
    public ClassName TypeName { get; }

    /// <summary>
    /// Gets a value indicating whether the type can have value 'null'.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// Gets a value indicating whether the type is an array. In this case, <see cref="IsNullable"/> applies to the element type.
    /// </summary>
    public bool IsArray { get; }

    /// <summary>
    /// Gets a value indicating whether the type is one of the simple predefined types.
    /// </summary>
    [JsonIgnore]
    public bool IsSimple
    {
        get
        {
            Debug.Assert(this != Other);

            if (IsArray)
                return false;

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

    /// <summary>
    /// Returns an array type from the current element type.
    /// </summary>
    public ExpressionType ToArrayType()
    {
        Debug.Assert(!IsArray);

        return new ExpressionType(TypeName, IsNullable, isArray: true);
    }

    /// <summary>
    /// Returns an element type from the current array type.
    /// </summary>
    public ExpressionType ToElementType()
    {
        Debug.Assert(IsArray);

        return new ExpressionType(TypeName, IsNullable, isArray: false);
    }

    /// <summary>
    /// Checks whether the provided type is an array of the current type.
    /// </summary>
    /// <param name="arrayType">The candidate array type.</param>
    public bool IsElementTypeOf(ExpressionType arrayType)
    {
        Debug.Assert(!IsArray);

        return arrayType.IsArray && TypeName == arrayType.TypeName && IsNullable == arrayType.IsNullable;
    }
}
