namespace ModelAnalyzer;

using System.Diagnostics;
using Newtonsoft.Json;

/// <summary>
/// Represents a new array as an expression.
/// </summary>
internal class NewArrayExpression : Expression, ILiteralExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType() => ArrayType;

    /// <inheritdoc/>
    public override LocationId LocationId { get; set; } = LocationId.CreateNew();

    /// <summary>
    /// Gets the array type.
    /// </summary>
    required public ExpressionType ArrayType { get; init; }

    /// <summary>
    /// Gets the array size.
    /// </summary>
    required public int ArraySize { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        Debug.Assert(ArrayType.IsArray);
        Debug.Assert(!ArrayType.IsNullable);
        Debug.Assert(ArraySize >= 0);

        return $"new {ArrayType.TypeName}[{ArraySize}]";
    }
}
