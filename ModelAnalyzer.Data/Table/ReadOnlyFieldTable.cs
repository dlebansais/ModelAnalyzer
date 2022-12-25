namespace ModelAnalyzer;

/// <summary>
/// Represents a read-only collection of fields and name keys.
/// </summary>
internal class ReadOnlyFieldTable : ReadOnlyNameAndItemTable<FieldName, Field>
{
    /// <summary>
    /// Gets the empty field table.
    /// </summary>
    public static ReadOnlyFieldTable Empty { get; } = new(new FieldTable());

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyFieldTable"/> class.
    /// </summary>
    public ReadOnlyFieldTable()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyFieldTable"/> class.
    /// </summary>
    /// <param name="table">The source table.</param>
    public ReadOnlyFieldTable(FieldTable table)
        : base(table)
    {
    }
}
