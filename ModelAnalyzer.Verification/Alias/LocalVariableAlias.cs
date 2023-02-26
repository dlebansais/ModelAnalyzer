namespace ModelAnalyzer;

/// <summary>
/// Represents an alias for a local variable.
/// </summary>
internal record LocalVariableAlias : VariableAlias
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalVariableAlias"/> class.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public LocalVariableAlias(Variable variable)
        : base(variable)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalVariableAlias"/> class.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <param name="index">The index.</param>
    public LocalVariableAlias(Variable variable, int index)
        : base(variable, index)
    {
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Variable.Name.Text}_{Index}";
    }
}
