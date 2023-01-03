namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported field.
/// </summary>
internal class UnsupportedField : IUnsupportedField
{
    /// <inheritdoc/>
    required public IVariableName Name { get; init; }

    /// <inheritdoc/>
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public ExpressionType Type => ExpressionType.Other;

    /// <inheritdoc/>
    public ILiteralExpression? Initializer => null;
}
