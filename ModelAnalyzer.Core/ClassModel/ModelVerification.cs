namespace DemoAnalyzer;

using System;
using System.Threading;

/// <summary>
/// Represents a model verification.
/// </summary>
internal class ModelVerification : IModelVerification
{
    /// <inheritdoc/>
    required public IClassModel ClassModel { get; init; }

    /// <inheritdoc/>
    public bool IsUpToDate { get; private set; }

    /// <inheritdoc/>
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
