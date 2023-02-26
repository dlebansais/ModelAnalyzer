namespace ModelAnalyzer;

/// <summary>
/// Represents a variable.
/// </summary>
internal record Variable : IVariable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Variable"/> class.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="type">The variable type.</param>
    public Variable(IVariableName name, ExpressionType type)
    {
        Name = name;
        Type = type;
    }

    /// <summary>
    /// Gets the variable name.
    /// </summary>
    public IVariableName Name { get; }

    /// <summary>
    /// Gets the variable type.
    /// </summary>
    public ExpressionType Type { get; }
}
