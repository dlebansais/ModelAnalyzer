namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of methods and name keys.
/// </summary>
internal class MethodTable : NameAndItemTable<MethodName, Method>
{
    /// <summary>
    /// Returns a read-only wrapper for the table.
    /// </summary>
    public ReadOnlyMethodTable AsReadOnly()
    {
        return new ReadOnlyMethodTable(this);
    }
}
