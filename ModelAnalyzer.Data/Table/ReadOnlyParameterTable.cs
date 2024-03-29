﻿namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of parameter and name keys.
/// </summary>
internal class ReadOnlyParameterTable : ReadOnlyNameAndItemTable<ParameterName, Parameter>
{
    /// <summary>
    /// Gets the empty parameter table.
    /// </summary>
    public static ReadOnlyParameterTable Empty { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyParameterTable"/> class.
    /// </summary>
    public ReadOnlyParameterTable()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyParameterTable"/> class.
    /// </summary>
    /// <param name="table">The source table.</param>
    public ReadOnlyParameterTable(ParameterTable table)
        : base(table)
    {
    }
}
