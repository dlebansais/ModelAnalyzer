namespace ModelAnalyzer;

/// <summary>
/// Provides information about a class invariant violation.
/// </summary>
public interface IInvariantViolation : IAssertionViolation
{
    /// <summary>
    /// Gets the invariant.
    /// </summary>
    IInvariant Invariant { get; }
}
