namespace ModelAnalyzer;

using System.Threading;

/// <summary>
/// Provides information about a class model.
/// </summary>
public interface IClassModel
{
    /// <summary>
    /// Gets the class name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets unsupported class elements.
    /// </summary>
    IUnsupported Unsupported { get; }

    /// <summary>
    /// Gets an event signaled when invariant violation has been verified.
    /// </summary>
    ManualResetEvent InvariantViolationVerified { get; }

    /// <summary>
    /// Gets a value indicating whether the class invariant is violated.
    /// </summary>
    bool IsInvariantViolated { get; }
}
