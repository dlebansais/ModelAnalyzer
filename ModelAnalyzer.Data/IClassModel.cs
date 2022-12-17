namespace ModelAnalyzer;

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
    Unsupported Unsupported { get; }

    /// <summary>
    /// Gets a value indicating whether the model is verified.
    /// </summary>
    bool IsVerified { get; }

    /// <summary>
    /// Gets a value indicating whether the invariant is violated.
    /// </summary>
    bool IsInvariantViolated { get; }
}
