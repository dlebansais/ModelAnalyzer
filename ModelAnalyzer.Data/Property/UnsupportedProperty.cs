namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported property.
/// </summary>
internal class UnsupportedProperty : IUnsupportedProperty
{
    /// <inheritdoc/>
    public IVariableName Name => new PropertyName() { Text = "*" };

    /// <inheritdoc/>
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public ExpressionType Type => ExpressionType.Other;

    /// <inheritdoc/>
    public ILiteralExpression? Initializer => null;
}
