namespace ModelAnalyzer;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents data exchanged between the class model manager and the verifier.
/// </summary>
internal partial record ModelExchange
{
    /// <summary>
    /// Gets the class models.
    /// </summary>
    required public ClassModelTable ClassModelTable { get; init; }

    /// <summary>
    /// Gets the recive channel guid.
    /// </summary>
    required public Guid ReceiveChannelGuid { get; init; }
}
