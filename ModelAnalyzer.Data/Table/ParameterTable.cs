namespace ModelAnalyzer;

/// <summary>
/// Represents a collection of parameter and name keys.
/// </summary>
internal class ParameterTable : NameAndItemTable<ParameterName, Parameter>
{
    /// <summary>
    /// Gets the empty parameter table.
    /// </summary>
    public static ParameterTable Empty { get; } = new ParameterTable();
}
