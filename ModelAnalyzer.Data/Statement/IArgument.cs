namespace ModelAnalyzer;

/// <summary>
/// Provides information about an argument.
/// </summary>
public interface IArgument
{
    /// <summary>
    /// Gets the argument expression.
    /// </summary>
    IExpression Expression { get; }
}
