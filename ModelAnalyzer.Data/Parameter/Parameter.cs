namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a method parameter.
/// </summary>
[DebuggerDisplay("{ParameterName.Text}")]
internal class Parameter : IParameter, INameable<ParameterName>
{
    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    required public ParameterName Name { get; init; }

    /// <inheritdoc/>
    IVariableName IVariable.Name { get => Name; }

    /// <inheritdoc/>
    required public ExpressionType Type { get; init; }
}
