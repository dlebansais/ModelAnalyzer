namespace ModelAnalyzer;

/// <summary>
/// Represents an alias for a local variable.
/// </summary>
internal record LocalAlias : VariableAlias
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalAlias"/> class.
    /// </summary>
    /// <param name="local">The local variable.</param>
    public LocalAlias(Local local)
        : base(local)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalAlias"/> class.
    /// </summary>
    /// <param name="local">The local variable.</param>
    /// <param name="index">The index.</param>
    public LocalAlias(Local local, int index)
        : base(local, index)
    {
    }

    /// <inheritdoc/>
    public override VariableAlias Incremented()
    {
        return new LocalAlias((Local)Variable, Index + 1);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Variable.Name.Text}_{Index}";
    }
}
