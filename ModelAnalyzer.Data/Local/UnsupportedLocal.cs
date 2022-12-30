namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported local variable.
/// </summary>
public class UnsupportedLocal : IUnsupportedLocal
{
    /// <inheritdoc/>
    public IVariableName Name => new LocalName() { Text = "*" };

    /// <inheritdoc/>
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public ExpressionType Type => ExpressionType.Other;
}
