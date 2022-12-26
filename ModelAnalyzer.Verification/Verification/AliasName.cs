namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents an alias for a variable.
/// </summary>
internal record AliasName
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AliasName"/> class.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public AliasName(IVariable variable)
    {
        Variable = variable;
        Index = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AliasName"/> class.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <param name="index">The index.</param>
    public AliasName(IVariable variable, int index)
    {
        Variable = variable;
        Index = index;
    }

    /// <summary>
    /// Gets the variable.
    /// </summary>
    public IVariable Variable { get; }

    /// <summary>
    /// Gets the alias index.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Return the alias with <see cref="Index"/> incremented.
    /// </summary>
    public AliasName Incremented()
    {
        return new AliasName(Variable, Index + 1);
    }

    /// <summary>
    /// Merges the alias with another. If they are at the same level, make not change. Otherwise increment this alias.
    /// </summary>
    /// <param name="other">The other alias.</param>
    /// <param name="isUpdated"><see langword="true"/> upon return if this alias was incremented; otherwise, <see langword="false"/>.</param>
    public AliasName Merged(AliasName other, out bool isUpdated)
    {
        Debug.Assert(Variable == other.Variable);

        if (other.Index != Index)
        {
            isUpdated = true;
            return Incremented();
        }
        else
        {
            isUpdated = false;
            return this;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Variable.Name}_{Index}";
    }
}
