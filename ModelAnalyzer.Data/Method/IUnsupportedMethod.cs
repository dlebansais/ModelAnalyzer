namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an unsupported method.
/// </summary>
public interface IUnsupportedMethod : IMethod
{
    /// <summary>
    /// Gets the method location.
    /// </summary>
    Location Location { get; }
}
