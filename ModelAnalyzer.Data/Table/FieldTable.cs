namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of fields and name keys.
/// </summary>
internal class FieldTable : NameAndItemTable<FieldName, Field>
{
    /// <summary>
    /// Returns a read-only table with the same elements as this instance.
    /// </summary>
    public ReadOnlyFieldTable ToReadOnly()
    {
        return new ReadOnlyFieldTable(this);
    }
}
