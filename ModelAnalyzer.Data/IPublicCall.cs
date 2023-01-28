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

    /// <summary>
    /// Gets a value indicating whether the call is to a static method or function.
    /// </summary>
    bool IsStatic { get; }
}
