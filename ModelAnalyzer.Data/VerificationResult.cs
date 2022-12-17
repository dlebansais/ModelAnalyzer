namespace ModelAnalyzer;

/// <summary>
/// Represents the result of a verification.
/// </summary>
public class VerificationResult
{
    /// <summary>
    /// Gets the class name.
    /// </summary>
    required public string ClassName { get; init; }

    /// <summary>
    /// Gets a value indicating whether the invariant is violated.
    /// </summary>
    required public bool IsInvariantViolated { get; init; }
}
