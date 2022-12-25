namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of methods and name keys.
/// </summary>
internal class MethodTable : NameAndItemTable<MethodName, Method>
{
    /// <summary>
    /// Returns a read-only table with the same elements as this instance.
    /// </summary>
    public ReadOnlyMethodTable ToReadOnly()
    {
        return new ReadOnlyMethodTable(this);
    }
}
