﻿namespace ModelAnalyzer;

using Newtonsoft.Json;

/// <summary>
/// Represents a guarantee when a method returns.
/// </summary>
internal class Ensure : IEnsure
{
    /// <inheritdoc/>
    [JsonIgnore]
    required public string Text { get; init; }

    /// <summary>
    /// Gets the expression guaranteed to be true.
    /// </summary>
    required public Expression BooleanExpression { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}