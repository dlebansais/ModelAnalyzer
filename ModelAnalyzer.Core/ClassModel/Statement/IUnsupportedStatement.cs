namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an unsupported statement.
/// </summary>
public interface IUnsupportedStatement : IStatement
{
    /// <summary>
    /// Gets the statement location.
    /// </summary>
    Location Location { get; }
}
