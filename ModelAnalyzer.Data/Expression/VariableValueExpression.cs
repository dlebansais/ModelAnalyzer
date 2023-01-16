namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a variable as an expression.
/// </summary>
internal class VariableValueExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType()
    {
        Debug.Assert(VariablePath.Count >= 1);

        return VariablePath.Last().Type;
    }

    /// <summary>
    /// Gets the variable path.
    /// </summary>
    required public List<IVariable> VariablePath { get; init; }

    /// <summary>
    /// Gets the path location.
    /// </summary>
    [JsonIgnore]
    required public Location PathLocation { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Join(".", VariablePath.ConvertAll(item => item.Name.Text));
    }
}
