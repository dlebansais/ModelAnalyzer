namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents an unsupported local variable.
/// </summary>
internal class UnsupportedLocal : IUnsupportedLocal
{
    /// <inheritdoc/>
    public IVariableName Name => new LocalName() { Text = "*" };

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public ExpressionType Type => ExpressionType.Other;

    /// <inheritdoc/>
    public ILiteralExpression? Initializer => null;

    /// <inheritdoc/>
    required public ClassName ClassName { get; init; }

    /// <inheritdoc/>
    required public MethodName MethodName { get; init; }
}
