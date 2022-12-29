namespace ModelAnalyzer;

/// <summary>
/// Provides information about a guarantee violation.
/// </summary>
public interface IEnsureViolation : IAssertionViolation
{
    /// <summary>
    /// Gets the method where the ensure clause is found.
    /// </summary>
    IMethod Method { get; }

    /// <summary>
    /// Gets the ensure clause.
    /// </summary>
    IEnsure Ensure { get; }
}
