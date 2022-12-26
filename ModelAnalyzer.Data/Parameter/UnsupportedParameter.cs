namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported parameter.
/// </summary>
public class UnsupportedParameter : IUnsupportedParameter
{
    /// <inheritdoc/>
    public IVariableName VariableName => new ParameterName() { Text = "*" };

    /// <inheritdoc/>
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public ExpressionType VariableType => ExpressionType.Other;
}
