﻿namespace ModelAnalyzer;

/// <summary>
/// Represents the model of a class.
/// </summary>
internal partial record VerificationState
{
    /// <summary>
    /// Gets the class model exchanged.
    /// </summary>
    required public ClassModelExchange ClassModelExchange { get; init; }

    /// <summary>
    /// Gets a value indicating whether a request to verify the model has been sent.
    /// </summary>
    required public bool IsVerificationRequestSent { get; init; }

    /// <summary>
    /// Gets a value indicating whether the model is verified.
    /// </summary>
    required public bool IsVerified { get; init; }
}
