namespace ModelAnalyzer;

/// <summary>
/// Provides information about a variable.
/// </summary>
public interface IVariable
{
    /// <summary>
    /// Gets the variable name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the variable type.
    /// </summary>
    ExpressionType VariableType { get; }
}
