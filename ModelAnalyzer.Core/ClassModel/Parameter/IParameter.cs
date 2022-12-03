namespace DemoAnalyzer;

/// <summary>
/// Provides information about a method parameter.
/// </summary>
public interface IParameter : IVariable
{
    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    IParameterName ParameterName { get; }
}
