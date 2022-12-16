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

    /// <summary>
    /// Gets the variable.
    /// </summary>
    required public IVariable Variable { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Variable.Name;
    }

    /// <inheritdoc/>
    public override string ToSimpleString()
    {
        return ToString();
    }
}
