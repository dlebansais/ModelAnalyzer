namespace ModelAnalyzer;

/// <summary>
/// Verification errors.
/// </summary>
internal enum VerificationErrorType
{
    /// <summary>
    /// Status unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// No error.
    /// </summary>
    Success,

    /// <summary>
    /// Exception caught during analysis.
    /// </summary>
    Exception,

    /// <summary>
    /// Timeout during analysis.
    /// </summary>
    Timeout,

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

    /// <summary>
    /// Error in assume clause.
    /// </summary>
    AssumeError,
}
