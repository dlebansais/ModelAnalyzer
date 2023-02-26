namespace ModelAnalyzer;

/// <summary>
/// Represents an alias for a field variable.
/// </summary>
internal record FieldAlias : VariableAlias
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FieldAlias"/> class.
    /// </summary>
    /// <param name="field">The field variable.</param>
    public FieldAlias(Field field)
        : base(field)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldAlias"/> class.
    /// </summary>
    /// <param name="field">The field variable.</param>
    /// <param name="index">The index.</param>
    public FieldAlias(Field field, int index)
        : base(field, index)
    {
    }

    /// <inheritdoc/>
    public override VariableAlias Incremented()
    {
        return new FieldAlias((Field)Variable, Index + 1);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Variable.Name.Text}_{Index}";
    }
}
