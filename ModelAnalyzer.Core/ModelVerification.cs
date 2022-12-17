namespace ModelAnalyzer;

using System;
using System.Threading;

/// <summary>
/// Represents a model verification.
/// </summary>
internal class ModelVerification
{
    /// <summary>
    /// Gets the class name.
    /// </summary>
    required public string ClassName { get; init; }

    /// <summary>
    /// Gets a value indicating whether the class model is up-to-date.
    /// </summary>
    public bool IsUpToDate { get; private set; }

    /// <summary>
    /// Waits for the class model to be up to date.
    /// </summary>
    /// <param name="duration">The waiting duration.</param>
    /// <param name="isCompleted">True if the model is up to date upon return.</param>
    public void WaitForUpToDate(TimeSpan duration, out bool isCompleted)
    {
        isCompleted = PulseEvent.WaitOne(duration);
    }

    /// <summary>
    /// Sets the <see cref="IsUpToDate"/> flag to <see langword="true"/>.
    /// </summary>
    public void SetUpToDate()
    {
        IsUpToDate = true;
        PulseEvent.Set();
    }

    private AutoResetEvent PulseEvent = new(initialState: false);
}
