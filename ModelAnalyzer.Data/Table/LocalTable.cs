namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of local and name keys.
/// </summary>
internal class LocalTable : NameAndItemTable<LocalName, Local>
{
    /// <summary>
    /// Returns a read-only wrapper for the table.
    /// </summary>
    public ReadOnlyLocalTable AsReadOnly()
    {
        return new ReadOnlyLocalTable(this);
    }
}
