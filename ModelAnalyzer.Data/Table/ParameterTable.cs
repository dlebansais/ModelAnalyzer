namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of parameter and name keys.
/// </summary>
internal class ParameterTable : NameAndItemTable<ParameterName, Parameter>
{
    /// <summary>
    /// Returns a read-only wrapper for the table.
    /// </summary>
    public ReadOnlyParameterTable AsReadOnly()
    {
        return new ReadOnlyParameterTable(this);
    }
}
