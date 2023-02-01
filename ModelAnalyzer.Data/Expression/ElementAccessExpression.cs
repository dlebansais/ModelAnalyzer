namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents the element of an array variable as an expression.
/// </summary>
internal class ElementAccessExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType()
    {
        Debug.Assert(VariablePath.Count >= 1);

        ExpressionType LastType = VariablePath.Last().Type;
        Debug.Assert(LastType.IsArray);

        return LastType.ToElementType();
    }

    /// <inheritdoc/>
    public override LocationId LocationId { get; set; } = LocationId.CreateNew();

    /// <summary>
    /// Gets the variable path.
    /// </summary>
    required public List<IVariable> VariablePath { get; init; }

    /// <summary>
    /// Gets the path location.
    /// </summary>
    [JsonIgnore]
    required public Location PathLocation { get; init; }

    /// <summary>
    /// Gets the element index expression.
    /// </summary>
    required public Expression ElementIndex { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string VariableString = string.Join(".", VariablePath.ConvertAll(item => item.Name.Text));
        string IndexString = ElementIndex.ToString();
        return $"{VariableString}[{IndexString}]";
    }
}
