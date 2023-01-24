namespace ModelAnalyzer;

using System.Diagnostics;
using Newtonsoft.Json;

/// <summary>
/// Represents a variable as an expression.
/// </summary>
internal class NewObjectExpression : Expression, ILiteralExpression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType() => ObjectType;

    /// <summary>
    /// Gets the variable.
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
