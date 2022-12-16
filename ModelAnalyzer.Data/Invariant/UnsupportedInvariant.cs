﻿namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported invariant.
/// </summary>
public class UnsupportedInvariant : IUnsupportedInvariant
{
    /// <inheritdoc/>
    required public string Text { get; init; }

    /// <inheritdoc/>
    required public Location Location { get; init; }
}
