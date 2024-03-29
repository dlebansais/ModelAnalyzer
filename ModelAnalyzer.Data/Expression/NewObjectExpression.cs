﻿namespace ModelAnalyzer;

using System.Diagnostics;
using Newtonsoft.Json;

/// <summary>
/// Represents a new object as an expression.
/// </summary>
internal class NewObjectExpression : Expression, ILiteralExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType() => ObjectType;

    /// <inheritdoc/>
    public override LocationId LocationId { get; set; } = LocationId.CreateNew();

    /// <summary>
    /// Gets the object type.
    /// </summary>
    required public ExpressionType ObjectType { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        Debug.Assert(!ObjectType.IsSimple);
        Debug.Assert(!ObjectType.IsNullable);

        return $"new {ObjectType.TypeName}()";
    }
}
