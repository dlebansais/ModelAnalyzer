namespace DemoAnalyzer;

using System;
using System.Threading;

/// <summary>
/// Represents a model verification.
/// </summary>
public class ModelVerification
{
    /// <summary>
    /// Gets the class model.
    /// </summary>
    required public IClassModel ClassModel { get; init; }

    /// <summary>
    /// Gets a value indicating whether the class model is up-to-date.
    /// </summary>
    public bool IsUpToDate { get; private set; }

    /// <summary>
    /// Sets the <see cref="IsUpToDate"/> flag to <see langword="true"/>.
    /// </summary>
    internal void SetUpToDate()
    {
        IsUpToDate = true;
        PulseEvent.Set();
    }

    /// <summary>
    /// Waits for the class model to be up to date.
    /// </summary>
    /// <param name="duration">The duration.</param>
    public void WaitForUpToDate(TimeSpan duration)
    {
        bool IsCompleted = PulseEvent.WaitOne(duration);
    }

    private AutoResetEvent PulseEvent = new(initialState: false);
}
