namespace ModelAnalyzer;

/// <summary>
/// Represents a requirement violation.
/// </summary>
internal class RequireViolation : IRequireViolation
{
    /// <inheritdoc/>
    required public IMethod Method { get; init; }

    /// <inheritdoc/>
    required public IRequire Require { get; init; }
}
