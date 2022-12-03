﻿namespace DemoAnalyzer;

/// <summary>
/// Provides information about a method.
/// </summary>
public interface IMethod
{
    /// <summary>
    /// Gets the method name.
    /// </summary>
    IMethodName MethodName { get; }
}
