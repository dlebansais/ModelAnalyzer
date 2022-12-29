namespace ModelAnalyzer;

/// <summary>
/// Provides information about a requirement violation.
/// </summary>
public interface IRequireViolation : IAssertionViolation
{
    /// <summary>
    /// Gets the method where the require clause is found.
    /// </summary>
    IMethod Method { get; }

    /// <summary>
    /// Gets the require clause.
    /// </summary>
    IRequire Require { get; }
}
