namespace ModelAnalyzer;

/// <summary>
/// Provides information about a method parameter.
/// </summary>
public interface IParameter : IVariable
{
    /// <summary>
    /// Gets the parameter host method name.
    /// </summary>
    MethodName MethodName { get; }
}
