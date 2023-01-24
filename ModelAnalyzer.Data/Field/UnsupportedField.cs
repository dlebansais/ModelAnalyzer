namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents an unsupported field.
/// </summary>
internal class UnsupportedField : IUnsupportedField
{
    /// <inheritdoc/>
    required public IVariableName Name { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public ExpressionType Type => ExpressionType.Other;

    /// <inheritdoc/>
    public ILiteralExpression? Initializer => null;
}
