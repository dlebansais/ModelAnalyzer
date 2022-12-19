namespace ModelAnalyzer;

using System;

/// <summary>
/// Represents data exchanged between the class model manager and the verifier.
/// </summary>
internal partial record ClassModelExchange
{
    /// <summary>
    /// Gets the class model.
    /// </summary>
    required public ClassModel ClassModel { get; init; }

    /// <summary>
    /// Gets the recive channel guid.
    /// </summary>
    required public Guid ReceiveChannelGuid { get; init; }
}
