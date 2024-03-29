﻿namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents the model of a class.
/// </summary>
internal partial record VerificationState
{
    /// <summary>
    /// Gets the class model exchanged.
    /// </summary>
    required public ModelExchange ModelExchange { get; init; }

    /// <summary>
    /// Gets a value indicating whether a request to verify the model has been sent.
    /// </summary>
    required public bool IsVerificationRequestSent { get; init; }

    /// <summary>
    /// Gets the verification result.
    /// </summary>
    required public Dictionary<ClassName, VerificationResult> VerificationResultTable { get; init; }
}
