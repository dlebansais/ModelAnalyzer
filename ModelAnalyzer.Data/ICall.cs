namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about a call to a function or method.
/// </summary>
internal interface ICall
{
    /// <summary>
    /// Gets the name of the class containing the called method.
    /// <see cref="ClassName.Empty"/> for a non-static private call.
    /// </summary>
    ClassName ClassName { get; }

    /// <summary>
    /// Gets the function or method name.
    /// </summary>
    MethodName MethodName { get; }

    /// <summary>
    /// Gets the list of arguments.
    /// </summary>
    List<Argument> ArgumentList { get; }

    /// <summary>
    /// Gets the function or method name location.
    /// </summary>
    Location NameLocation { get; }

    /// <summary>
    /// Gets the name of the class where the caller is found.
    /// </summary>
    ClassName CallerClassName { get; }

    /// <summary>
    /// Gets a value indicating whether the call is to a static method or function.
    /// </summary>
    bool IsStatic { get; }
}
