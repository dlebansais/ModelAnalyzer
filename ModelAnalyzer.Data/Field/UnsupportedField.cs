namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported field.
/// </summary>
public class UnsupportedField : IUnsupportedField
{
    /// <inheritdoc/>
    public string Name => "*";

    /// <inheritdoc/>
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public ExpressionType VariableType => ExpressionType.Other;
}
