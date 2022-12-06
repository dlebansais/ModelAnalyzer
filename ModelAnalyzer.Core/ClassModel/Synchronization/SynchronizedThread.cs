namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Represents a thread that synchronizes access to an object.
/// </summary>
/// <typeparam name="TSynch">The type of the synchronization object.</typeparam>
/// <typeparam name="TItem">The type of the object being synchronized.</typeparam>
internal class SynchronizedThread<TSynch, TItem>
    where TItem : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizedThread{TSynch,TItem}"/> class.
    /// </summary>
    /// <param name="context">The synchronized context.</param>
    /// <param name="callback">The callback.</param>
    public SynchronizedThread(ISynchronizedContext<TSynch, TItem> context, Action<IDictionary<TSynch, TItem>> callback)
    {
        Context = context;
        Callback = callback;
    }

    /// <summary>
    /// Gets the processed object.
    /// </summary>
    public ISynchronizedContext<TSynch, TItem> Context { get; }

    /// <summary>
    /// Gets the callback.
    /// </summary>
    public Action<IDictionary<TSynch, TItem>> Callback { get; }

    /// <summary>
    /// Starts the thread or ensures the thread will be restarted if currently executing.
    /// </summary>
    public void Start()
    {
        lock (Context.Lock)
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
        IDictionary<TSynch, TItem> CloneTable;

        lock (Context.Lock)
        {
            Context.CloneAndRemove(out CloneTable);
            ThreadShouldBeRestarted = false;
        }

        Callback(CloneTable);

        bool Restart = false;

        lock (Context.Lock)
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
