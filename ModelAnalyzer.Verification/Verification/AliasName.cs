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
    /// Return the alias with <see cref="Index"/> incremented.
    /// </summary>
    public AliasName Incremented()
    {
        AliasName Result = new() { VariableName = VariableName };
        Result.Index = Index + 1;

        return Result;
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

    private int Index;
}
