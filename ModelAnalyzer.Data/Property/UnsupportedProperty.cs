namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents an unsupported property.
/// </summary>
internal class UnsupportedProperty : IUnsupportedProperty
{
    /// <inheritdoc/>
    public IVariableName Name => new PropertyName() { Text = "*" };

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public ExpressionType Type => ExpressionType.Other;

    /// <inheritdoc/>
    public ILiteralExpression? Initializer => null;

    /// <inheritdoc/>
    required public ClassName ClassName { get; init; }
}
