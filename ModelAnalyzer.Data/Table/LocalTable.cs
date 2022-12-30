namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of local and name keys.
/// </summary>
internal class LocalTable : NameAndItemTable<LocalName, Local>
{
    /// <summary>
    /// Returns a read-only table with the same elements as this instance.
    /// </summary>
    public ReadOnlyLocalTable ToReadOnly()
    {
        return new ReadOnlyLocalTable(this);
    }
}
