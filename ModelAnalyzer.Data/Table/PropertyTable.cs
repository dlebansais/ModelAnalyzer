namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of propertys and name keys.
/// </summary>
internal class PropertyTable : NameAndItemTable<PropertyName, Property>
{
    /// <summary>
    /// Returns a read-only wrapper for the table.
    /// </summary>
    public ReadOnlyPropertyTable AsReadOnly()
    {
        return new ReadOnlyPropertyTable(this);
    }
}
