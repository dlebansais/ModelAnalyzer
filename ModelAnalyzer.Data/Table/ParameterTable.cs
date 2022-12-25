namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of parameter and name keys.
/// </summary>
internal class ParameterTable : NameAndItemTable<ParameterName, Parameter>
{
    /// <summary>
    /// Returns a read-only table with the same elements as this instance.
    /// </summary>
    public ReadOnlyParameterTable ToReadOnly()
    {
        return new ReadOnlyParameterTable(this);
    }
}
