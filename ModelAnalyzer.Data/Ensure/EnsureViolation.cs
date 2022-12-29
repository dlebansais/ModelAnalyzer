namespace ModelAnalyzer;

/// <summary>
/// Represents a guarantee violation.
/// </summary>
internal class EnsureViolation : IEnsureViolation
{
    /// <inheritdoc/>
    required public IMethod Method { get; init; }

    /// <inheritdoc/>
    required public IEnsure Ensure { get; init; }
}
