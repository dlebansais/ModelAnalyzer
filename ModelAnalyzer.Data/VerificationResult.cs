﻿namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents information about the result of a class model verification.
/// </summary>
internal record VerificationResult
{
    /// <summary>
    /// Gets the defaut value for no error.
    /// </summary>
    public static VerificationResult Default { get; } = new()
    {
        ErrorType = VerificationErrorType.Unknown,
        ClassName = ClassName.Empty,
        MethodName = string.Empty,
        LocationId = LocationId.None,
        ErrorText = string.Empty,
    };

    /// <summary>
    /// Gets the error type.
    /// </summary>
    required public VerificationErrorType ErrorType { get; init; }

    /// <summary>
    /// Gets the class name.
    /// </summary>
    required public ClassName ClassName { get; init; }

    /// <summary>
    /// Gets the method name.
    /// </summary>
    required public string MethodName { get; init; }

    /// <summary>
    /// Gets the location the clause with error.
    /// </summary>
    required public LocationId LocationId { get; init; }

    /// <summary>
    /// Gets the text of the expression with error.
    /// </summary>
    required public string ErrorText { get; init; }

    /// <summary>
    /// Gets a value indicating whether the verification was successful.
    /// </summary>
    public bool IsSuccess
    {
        get
        {
            Debug.Assert(ErrorType != VerificationErrorType.Unknown);
            return ErrorType == VerificationErrorType.Success;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the verification failed.
    /// </summary>
    public bool IsError
    {
        get
        {
            Debug.Assert(ErrorType != VerificationErrorType.Unknown);
            return ErrorType > VerificationErrorType.Success;
        }
    }
}
