namespace ModelAnalyzer;

/// <summary>
/// Provides information about a variable.
/// </summary>
public interface IVariable
{
    /// <summary>
    /// Gets the variable name.
    /// </summary>
    IVariableName Name { get; }

    /// <summary>
    /// Gets the variable type.
    /// </summary>
    ExpressionType Type { get; }
}
