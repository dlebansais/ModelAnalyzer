namespace ModelAnalyzer;

/// <summary>
/// Provides information about collection of class members.
/// </summary>
internal interface IMemberCollectionContext
{
    /// <summary>
    /// Gets the table of class fields.
    /// </summary>
    FieldTable FieldTable { get; }

    /// <summary>
    /// Gets the current method with parameters and locals.
    /// </summary>
    Method? HostMethod { get; }

    /// <summary>
    /// Gets the local variable that represents the value returned by a method.
    /// </summary>
    Local? ResultLocal { get; }
}
