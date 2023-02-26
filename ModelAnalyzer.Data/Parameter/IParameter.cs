namespace ModelAnalyzer;

/// <summary>
/// Provides information about a method parameter.
/// </summary>
public interface IParameter : IVariable
{
    /// <summary>
    /// Gets the local host method owner class name.
    /// </summary>
    ClassName ClassName { get; }

    /// <summary>
    /// Gets the parameter host method name.
    /// </summary>
    MethodName MethodName { get; }
}
