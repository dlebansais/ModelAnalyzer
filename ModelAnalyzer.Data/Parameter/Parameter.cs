namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a method parameter.
/// </summary>
[DebuggerDisplay("{ParameterName.Name}")]
internal class Parameter : IParameter, IVariable
{
    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    required public ParameterName ParameterName { get; init; }

    /// <inheritdoc/>
    public IVariableName VariableName { get => ParameterName; }

    /// <inheritdoc/>
    required public ExpressionType VariableType { get; init; }
}
