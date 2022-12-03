namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an unsupported invariant.
/// </summary>
public interface IUnsupportedInvariant : IInvariant
{
    /// <summary>
    /// Gets the invariant location.
    /// </summary>
    Location Location { get; }
}
