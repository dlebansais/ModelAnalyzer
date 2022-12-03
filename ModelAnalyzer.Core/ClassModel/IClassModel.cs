namespace DemoAnalyzer;

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
    /// Waits for the verification to complete.
    /// </summary>
    void WaitForVerifyCompleted();
}
