namespace DemoAnalyzer;

using System;
using System.Threading;

/// <summary>
/// Represents a thread that synchronizes access to an object.
/// </summary>
/// <typeparam name="T">The type of the synchronized object.</typeparam>
public class SynchronizedThread<T>
    where T : class, ICloneable, System.Collections.IList
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizedThread{T}"/> class.
    /// </summary>
    /// <param name="processedObject">The processed object.</param>
    /// <param name="callback">The callback.</param>
    public SynchronizedThread(T processedObject, Action<T> callback)
    {
        ProcessedObject = processedObject;
        Callback = callback;
    }

    /// <summary>
    /// Gets the processed object.
    /// </summary>
    public T ProcessedObject { get; }

    /// <summary>
    /// Gets the callback.
    /// </summary>
    public Action<T> Callback { get; }

    /// <summary>
    /// Starts the thread or ensures the thread will be restarted if currently executing.
    /// </summary>
    public void Start()
    {
        lock (ProcessedObject)
        {
            if (SynchronizationThread is null)
            {
                ThreadShouldBeRestarted = false;
                SynchronizationThread = new Thread(new ThreadStart(ExecuteThread));
                SynchronizationThread.Start();
            }
            else
                ThreadShouldBeRestarted = true;
        }
    }

    private void ExecuteThread()
    {
        T Clone;

        lock (ProcessedObject)
        {
            Clone = (T)ProcessedObject.Clone();
            ThreadShouldBeRestarted = false;
        }

        Callback(Clone);

        bool Restart = false;

        lock (ProcessedObject)
        {
            SynchronizationThread = null;

            if (ThreadShouldBeRestarted)
                Restart = true;
        }

        if (Restart)
        {
            ThreadShouldBeRestarted = false;
            SynchronizationThread = new Thread(new ThreadStart(ExecuteThread));
            SynchronizationThread.Start();
        }
    }

    private Thread? SynchronizationThread = null;
    private bool ThreadShouldBeRestarted;
}
