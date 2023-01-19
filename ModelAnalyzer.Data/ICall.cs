namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about a call to a function or method.
/// </summary>
internal interface ICall
{
    /// <summary>
    /// Gets the function or method name.
    /// </summary>
    MethodName Name { get; }

    /// <summary>
    /// Gets the list of arguments.
    /// </summary>
    List<Argument> ArgumentList { get; }

    /// <summary>
    /// Gets the function or method name location.
    /// </summary>
    Location NameLocation { get; }
}
