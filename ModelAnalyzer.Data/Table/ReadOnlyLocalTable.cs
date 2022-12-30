namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of local and name keys.
/// </summary>
internal class ReadOnlyLocalTable : ReadOnlyNameAndItemTable<LocalName, Local>
{
    /// <summary>
    /// Gets the empty local table.
    /// </summary>
    public static ReadOnlyLocalTable Empty { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyLocalTable"/> class.
    /// </summary>
    public ReadOnlyLocalTable()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyLocalTable"/> class.
    /// </summary>
    /// <param name="table">The source table.</param>
    public ReadOnlyLocalTable(LocalTable table)
        : base(table)
    {
    }
}
