namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported statement.
/// </summary>
internal class UnsupportedStatement : IUnsupportedStatement
{
    /// <inheritdoc/>
    required public Location Location { get; init; }
}
