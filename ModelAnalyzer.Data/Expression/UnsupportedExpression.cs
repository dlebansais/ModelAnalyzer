namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported expression.
/// </summary>
public class UnsupportedExpression : IUnsupportedExpression
{
    /// <inheritdoc/>
    required public Location Location { get; init; }

    /// <summary>
    /// Gets the expression type.
    /// </summary>
    /// <param name="fieldTable">The table of fields.</param>
    /// <param name="parameterTable">The table of parameters.</param>
    internal ExpressionType GetExpressionType(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable) => ExpressionType.Other;
}
