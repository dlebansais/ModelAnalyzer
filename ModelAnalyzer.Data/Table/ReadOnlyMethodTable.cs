namespace ModelAnalyzer;

/// <summary>
/// Represents a read-only collection of methods and name keys.
/// </summary>
internal class ReadOnlyMethodTable : ReadOnlyNameAndItemTable<MethodName, Method>
{
    /// <summary>
    /// Gets the empty method table.
    /// </summary>
    public static ReadOnlyMethodTable Empty { get; } = new(new MethodTable());

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyMethodTable"/> class.
    /// </summary>
    public ReadOnlyMethodTable()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyMethodTable"/> class.
    /// </summary>
    /// <param name="table">The source table.</param>
    public ReadOnlyMethodTable(MethodTable table)
        : base(table)
    {
    }
}
