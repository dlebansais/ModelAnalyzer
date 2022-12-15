namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an unsupported parameter.
/// </summary>
public interface IUnsupportedParameter : IParameter
{
    /// <summary>
    /// Gets the parameter location.
    /// </summary>
    Location Location { get; }
}
