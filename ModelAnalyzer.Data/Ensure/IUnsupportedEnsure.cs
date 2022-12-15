namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an unsupported guarantee assertion.
/// </summary>
public interface IUnsupportedEnsure : IEnsure
{
    /// <summary>
    /// Gets the error location.
    /// </summary>
    Location Location { get; }
}
