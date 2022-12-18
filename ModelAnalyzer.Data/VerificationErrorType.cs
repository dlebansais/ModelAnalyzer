namespace ModelAnalyzer;

/// <summary>
/// Verification errors.
/// </summary>
internal enum VerificationErrorType
{
    /// <summary>
    /// No error.
    /// </summary>
    Success,

    /// <summary>
    /// Error during analysis.
    /// </summary>
    Abort,

    /// <summary>
    /// Error in require clause.
    /// </summary>
    RequireError,

    /// <summary>
    /// Error in ensure clause.
    /// </summary>
    EnsureError,

    /// <summary>
    /// Error in invariant clause.
    /// </summary>
    InvariantError,
}
