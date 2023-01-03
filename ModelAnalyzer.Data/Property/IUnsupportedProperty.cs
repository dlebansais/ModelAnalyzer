namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an unsupported property.
/// </summary>
public interface IUnsupportedProperty : IProperty
{
    /// <summary>
    /// Gets the property location.
    /// </summary>
    Location Location { get; }
}
