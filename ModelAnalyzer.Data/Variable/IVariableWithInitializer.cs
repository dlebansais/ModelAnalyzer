namespace ModelAnalyzer;

/// <summary>
/// Provides information about a variable that can be initialized.
/// </summary>
public interface IVariableWithInitializer : IVariable
{
    /// <summary>
    /// Gets the initializer.
    /// </summary>
    ILiteralExpression? Initializer { get; }
}
