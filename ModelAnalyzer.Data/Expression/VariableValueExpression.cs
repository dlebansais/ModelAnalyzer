namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
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
    public override ExpressionType GetExpressionType(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable)
    {
        return GetVariable(fieldTable, parameterTable).VariableType;
    }

    /// <summary>
    /// Gets the variable.
    /// </summary>
    required public IVariableName VariableName { get; init; }

    /// <summary>
    /// Gets a variable from its name.
    /// </summary>
    /// <param name="fieldTable">The table of fields.</param>
    /// <param name="parameterTable">The table of parameters.</param>
    public IVariable GetVariable(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable)
    {
        return ClassModel.GetVariable(fieldTable, parameterTable, VariableName);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return VariableName.Text;
    }
}
