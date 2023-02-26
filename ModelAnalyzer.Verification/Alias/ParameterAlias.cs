namespace ModelAnalyzer;

/// <summary>
/// Represents an alias for a parameter variable.
/// </summary>
internal record ParameterAlias : VariableAlias
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterAlias"/> class.
    /// </summary>
    /// <param name="parameter">The parameter variable.</param>
    public ParameterAlias(Parameter parameter)
        : base(parameter)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterAlias"/> class.
    /// </summary>
    /// <param name="parameter">The parameter variable.</param>
    /// <param name="index">The index.</param>
    public ParameterAlias(Parameter parameter, int index)
        : base(parameter, index)
    {
    }

    /// <inheritdoc/>
    public override VariableAlias Incremented()
    {
        return new ParameterAlias((Parameter)Variable, Index + 1);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Variable.Name.Text}_{Index}";
    }
}
