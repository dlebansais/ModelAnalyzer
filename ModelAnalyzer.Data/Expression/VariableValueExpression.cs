namespace ModelAnalyzer;

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
    public override ExpressionType GetExpressionType(ReadOnlyFieldTable fieldTable, Method? hostMethod, Local? resultLocal)
    {
        return ClassModel.GetVariable(fieldTable, hostMethod, resultLocal, VariableName).Type;
    }

    /// <summary>
    /// Gets the variable.
    /// </summary>
    required public IVariableName VariableName { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return VariableName.Text;
    }
}
