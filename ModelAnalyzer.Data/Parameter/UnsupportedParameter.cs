namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents an unsupported parameter.
/// </summary>
internal class UnsupportedParameter : IUnsupportedParameter
{
    /// <inheritdoc/>
    public IVariableName Name => new ParameterName() { Text = "*" };

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public ExpressionType Type => ExpressionType.Other;

    /// <inheritdoc/>
    required public ClassName ClassName { get; init; }

    /// <inheritdoc/>
    required public MethodName MethodName { get; init; }
}
