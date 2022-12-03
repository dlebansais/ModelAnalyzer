namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an unsupported requirement assertion.
/// </summary>
public interface IUnsupportedRequire : IRequire
{
    /// <summary>
    /// Gets the require location.
    /// </summary>
    Location Location { get; }
}
