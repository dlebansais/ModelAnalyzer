namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a requirement violation.
/// </summary>
internal class RequireViolation : IRequireViolation
{
    /// <inheritdoc/>
    required public IMethod Method { get; init; }

    /// <inheritdoc/>
    required public string Text { get; init; }

    /// <inheritdoc/>
    required public Location NameLocation { get; init; }

    /// <inheritdoc/>
    required public IStatement? Statement { get; init; }

    /// <inheritdoc/>
    required public IExpression? Expression { get; init; }

    /// <inheritdoc/>
    required public IRequire? Require { get; init; }
}
