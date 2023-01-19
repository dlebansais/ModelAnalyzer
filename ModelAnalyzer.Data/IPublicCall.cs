namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides information about a public call to a function or method.
/// </summary>
internal interface IPublicCall
{
    /// <summary>
    /// Gets the variable path.
    /// </summary>
    List<IVariable> VariablePath { get; }
}
