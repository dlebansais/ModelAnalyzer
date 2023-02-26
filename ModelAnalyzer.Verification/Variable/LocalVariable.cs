namespace ModelAnalyzer;

/// <summary>
/// Represents a variable.
/// </summary>
internal record LocalVariable : Variable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalVariable"/> class.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="type">The variable type.</param>
    public LocalVariable(IVariableName name, ExpressionType type)
        : base(name, type)
    {
    }
}
