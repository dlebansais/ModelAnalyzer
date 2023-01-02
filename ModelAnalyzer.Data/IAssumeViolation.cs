namespace ModelAnalyzer;

/// <summary>
/// Provides information about a flow check violation.
/// </summary>
public interface IAssumeViolation : IAssertionViolation
{
    /// <summary>
    /// Gets the method where the violation is found. Null if within an invariant clause.
    /// </summary>
    IMethod? Method { get; }

    /// <summary>
    /// Gets the error text.
    /// </summary>
    string Text { get; }
}
