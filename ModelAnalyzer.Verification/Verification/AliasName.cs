namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents an alias for a variable name.
/// </summary>
internal record AliasName
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AliasName"/> class.
    /// </summary>
    /// <param name="variableName">The variable name.</param>
    public AliasName(string variableName)
    {
        VariableName = variableName;
        Index = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AliasName"/> class.
    /// </summary>
    /// <param name="variableName">The variable name.</param>
    /// <param name="index">The index.</param>
    public AliasName(string variableName, int index)
    {
        VariableName = variableName;
        Index = index;
    }

    /// <summary>
    /// Gets the variable name.
    /// </summary>
    public string VariableName { get; }

    /// <summary>
    /// Gets the alias index.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Return the alias with <see cref="Index"/> incremented.
    /// </summary>
    public AliasName Incremented()
    {
        return new AliasName(VariableName, Index + 1);
    }

    /// <summary>
    /// Merges the alias with another. If they are at the same level, make not change. Otherwise increment this alias.
    /// </summary>
    /// <param name="other">The other alias.</param>
    /// <param name="isUpdated"><see langword="true"/> upon return if this alias was incremented; otherwise, <see langword="false"/>.</param>
    public AliasName Merged(AliasName other, out bool isUpdated)
    {
        Debug.Assert(VariableName == other.VariableName);

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
        return $"{VariableName}_{Index}";
    }
}
