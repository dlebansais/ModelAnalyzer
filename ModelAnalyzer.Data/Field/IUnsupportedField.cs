namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an unsupported field.
/// </summary>
public interface IUnsupportedField : IField
{
    /// <summary>
    /// Gets the field location.
    /// </summary>
    Location Location { get; }
}
