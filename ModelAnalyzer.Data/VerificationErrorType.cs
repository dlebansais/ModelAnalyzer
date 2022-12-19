﻿namespace ModelAnalyzer;

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