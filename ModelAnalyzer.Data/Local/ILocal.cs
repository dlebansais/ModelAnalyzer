namespace ModelAnalyzer;

/// <summary>
/// Provides information about a local variable.
/// </summary>
public interface ILocal : IVariableWithInitializer
{
    /// <summary>
    /// Gets the local host method name.
    /// </summary>
    MethodName MethodName { get; }
}
