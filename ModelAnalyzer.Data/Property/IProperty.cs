namespace ModelAnalyzer;

/// <summary>
/// Provides information about a property.
/// </summary>
public interface IProperty : IVariableWithInitializer
{
    /// <summary>
    /// Gets the property owner class name.
    /// </summary>
    ClassName ClassName { get; }
}
