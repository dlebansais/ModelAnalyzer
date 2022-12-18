namespace ModelAnalyzer;

/// <summary>
/// Represents information about an error encountered during verification.
/// </summary>
internal record VerificationError
{
    /// <summary>
    /// Gets the defaut value for no error.
    /// </summary>
    public static VerificationError None { get; } = new()
    {
        ErrorType = VerificationErrorType.Success,
        ClassName = string.Empty,
        MethodName = string.Empty,
        ErrorIndex = -1,
    };

    /// <summary>
    /// Gets the error type.
    /// </summary>
    required public VerificationErrorType ErrorType { get; init; }

    /// <summary>
    /// Gets the class name.
    /// </summary>
    required public string ClassName { get; init; }

    /// <summary>
    /// Gets the method name.
    /// </summary>
    required public string MethodName { get; init; }

    /// <summary>
    /// Gets the index of the clause with error.
    /// </summary>
    required public int ErrorIndex { get; init; }

    /// <summary>
    /// Gets a value indicating whether the verification was successful.
    /// </summary>
    public bool IsSuccess { get => ErrorType == VerificationErrorType.Success; }

    /// <summary>
    /// Gets a value indicating whether the verification failed.
    /// </summary>
    public bool IsError { get => ErrorType != VerificationErrorType.Success; }
}
