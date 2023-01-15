namespace ModelAnalyzer;

using System.Collections.Generic;

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

    /// <summary>
    /// Gets the <see cref="Ensure.ResultKeyword"/> local if it exists, otherwise returns null.
    /// </summary>
    public Local? GetResultLocal()
    {
        foreach (KeyValuePair<LocalName, Local> Entry in List)
            if (Entry.Key.Text == Ensure.ResultKeyword)
                return Entry.Value;

        return null;
    }
}
