namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents an alias for a variable name.
/// </summary>
internal record AliasName
{
    /// <summary>
    /// Gets the variable name.
    /// </summary>
    required public string VariableName { get; init; }

    /// <summary>
    /// Gets the alias.
    /// </summary>
    public string Alias { get { return $"{VariableName}_{Index}"; } }

    /// <summary>
    /// Increment the alias so that <see cref="Alias"/> is changed.
    /// </summary>
    public void Increment()
    {
        Index++;
    }

    /// <summary>
    /// Merges the alias with another. If they are at the same level, make not change. Otherwise increment this alias.
    /// </summary>
    /// <param name="other">The other alias.</param>
    /// <param name="isUpdated"><see langword="true"/> upon return if this alias was incremented; otherwise, <see langword="false"/>.</param>
    public void Merge(AliasName other, out bool isUpdated)
    {
        Debug.Assert(VariableName == other.VariableName);

        if (other.Index != Index)
        {
            isUpdated = true;
            Increment();
        }
        else
            isUpdated = false;
    }

    private int Index;
}
