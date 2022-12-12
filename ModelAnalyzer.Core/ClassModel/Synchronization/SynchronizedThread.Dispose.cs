namespace ModelAnalyzer;

using System;
using System.Threading;

/// <inheritdoc/>
internal partial class SynchronizedThread<TSynch, TItem> : IDisposable
    where TItem : class
{
    /// <summary>
    /// Called when an object should release its resources.
    /// </summary>
    /// <param name="isDisposing">Indicates if resources must be disposed now.</param>
    protected virtual void Dispose(bool isDisposing)
    {
        if (!IsDisposed)
        {
            IsDisposed = true;

            if (isDisposing)
                DisposeNow();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="SynchronizedThread{TSynch,TItem}"/> class.
    /// </summary>
    ~SynchronizedThread()
    {
        Dispose(false);
    }

    /// <summary>
    /// True after <see cref="Dispose(bool)"/> has been invoked.
    /// </summary>
    private bool IsDisposed;

    /// <summary>
    /// Disposes of every reference that must be cleaned up.
    /// </summary>
    private void DisposeNow()
    {
        Thread? DisposedThread;

        lock (Context.Lock)
        {
            DisposedThread = SynchronizationThread;
            ThreadShouldBeRestarted = false;
        }

        if (DisposedThread is not null)
            DisposedThread.Join();
    }
}
