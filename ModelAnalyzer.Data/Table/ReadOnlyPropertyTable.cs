namespace ModelAnalyzer;

/// <summary>
/// Represents a read-only collection of propertys and name keys.
/// </summary>
internal class ReadOnlyPropertyTable : ReadOnlyNameAndItemTable<PropertyName, Property>
{
    /// <summary>
    /// Gets the empty property table.
    /// </summary>
    public static ReadOnlyPropertyTable Empty { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyPropertyTable"/> class.
    /// </summary>
    public ReadOnlyPropertyTable()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyPropertyTable"/> class.
    /// </summary>
    /// <param name="table">The source table.</param>
    public ReadOnlyPropertyTable(PropertyTable table)
        : base(table)
    {
    }
}
