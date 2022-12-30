namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a method call argument.
/// </summary>
internal class Argument : IArgument
{
    /// <inheritdoc/>
    required public IExpression Expression { get; init; }

    /// <summary>
    /// Gets the argument location.
    /// </summary>
    [JsonIgnore]
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Expression.ToString();
    }
}
