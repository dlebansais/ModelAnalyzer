namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides information about collection of class members.
/// </summary>
internal interface IMemberCollectionContext
{
    /// <summary>
    /// Gets the list of class fields.
    /// </summary>
    List<Field> GetFields();

    /// <summary>
    /// Gets the current method with parameters and locals.
    /// </summary>
    Method? HostMethod { get; }

    /// <summary>
    /// Gets the local variable that represents the value returned by a method.
    /// </summary>
    Local? ResultLocal { get; }
}
