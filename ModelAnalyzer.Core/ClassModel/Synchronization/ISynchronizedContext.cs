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
    /// <param name="cloneTable">The table of synchronization objects.</param>
    void CloneAndRemove(out IDictionary<TSynch, TItem> cloneTable);
}
