namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported parameter.
/// </summary>
public class UnsupportedParameter : IUnsupportedParameter
{
    /// <inheritdoc/>
    public IVariableName Name => new ParameterName() { Text = "*" };

    /// <inheritdoc/>
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public ExpressionType Type => ExpressionType.Other;
}
