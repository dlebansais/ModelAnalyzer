namespace DemoAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides information about a context for synchronized objects.
/// </summary>
/// <typeparam name="TSynch">The type of the synchronization object.</typeparam>
/// <typeparam name="TItem">The type of the object being synchronized.</typeparam>
public interface ISynchronizedContext<TSynch, TItem>
    where TItem : class
{
    /// <summary>
    /// Gets the synchronization lock.
    /// </summary>
    object Lock { get; }

    /// <summary>
    /// Clones the synchronized context and removes cloned elements.
    /// </summary>
    /// <param name="syncList">The list of synchronization objects.</param>
    /// <param name="itemList">The list of clones of objects being synchronized.</param>
    void CloneAndRemove(out List<TSynch> syncList, out List<TItem> itemList);
}
