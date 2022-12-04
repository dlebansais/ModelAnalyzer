namespace DemoAnalyzer;

using System;

/// <summary>
/// Provides information about a class model verification.
/// </summary>
public interface IModelVerification
{
    /// <summary>
    /// Gets the class model.
    /// </summary>
    IClassModel ClassModel { get; }

    /// <summary>
    /// Gets a value indicating whether the class model is up-to-date.
    /// </summary>
    bool IsUpToDate { get; }

    /// <summary>
    /// Waits for the class model to be up to date.
    /// </summary>
    /// <param name="duration">The duration.</param>
    void WaitForUpToDate(TimeSpan duration);
}
