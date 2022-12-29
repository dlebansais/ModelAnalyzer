namespace ModelAnalyzer;

/// <summary>
/// Represents a class invariant violation.
/// </summary>
internal class InvariantViolation : IInvariantViolation
{
    /// <inheritdoc/>
    required public IInvariant Invariant { get; init; }
}
