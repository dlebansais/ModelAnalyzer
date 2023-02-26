namespace ModelAnalyzer;

/// <summary>
/// Provides information about a field.
/// </summary>
public interface IField : IVariableWithInitializer
{
    /// <summary>
    /// Gets the field owner class name.
    /// </summary>
    ClassName ClassName { get; }
}
