namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an unsupported local variable.
/// </summary>
public interface IUnsupportedLocal : ILocal
{
    /// <summary>
    /// Gets the local variable location.
    /// </summary>
    Location Location { get; }
}
