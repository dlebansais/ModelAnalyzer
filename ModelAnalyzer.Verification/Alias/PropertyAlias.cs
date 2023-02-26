namespace ModelAnalyzer;

/// <summary>
/// Represents an alias for a property variable.
/// </summary>
internal record PropertyAlias : VariableAlias
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyAlias"/> class.
    /// </summary>
    /// <param name="property">The property variable.</param>
    public PropertyAlias(Property property)
        : base(property)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyAlias"/> class.
    /// </summary>
    /// <param name="property">The property variable.</param>
    /// <param name="index">The index.</param>
    public PropertyAlias(Property property, int index)
        : base(property, index)
    {
    }

    /// <inheritdoc/>
    public override VariableAlias Incremented()
    {
        return new PropertyAlias((Property)Variable, Index + 1);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Variable.Name.Text}_{Index}";
    }
}
