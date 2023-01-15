namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides information about collection of class members.
/// </summary>
internal interface IMemberCollectionContext
{
    /// <summary>
    /// Gets the list of class properties.
    /// </summary>
    List<Property> GetProperties();

    /// <summary>
    /// Gets the list of class fields.
    /// </summary>
    List<Field> GetFields();

    /// <summary>
    /// Gets the list of class methods.
    /// </summary>
    List<Method> GetMethods();

    /// <summary>
    /// Gets the current method with parameters and locals.
    /// </summary>
    Method? HostMethod { get; }
}
