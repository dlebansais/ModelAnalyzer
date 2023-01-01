namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of fields and name keys.
/// </summary>
internal class FieldTable : NameAndItemTable<FieldName, Field>
{
    /// <summary>
    /// Returns a read-only wrapper for the table.
    /// </summary>
    public ReadOnlyFieldTable AsReadOnly()
    {
        return new ReadOnlyFieldTable(this);
    }
}
