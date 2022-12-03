namespace DemoAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents a collection of items and name keys.
/// </summary>
/// <typeparam name="TName">The name type.</typeparam>
/// <typeparam name="TItem">The item type.</typeparam>
internal class NameAndItemTable<TName, TItem> : IEnumerable<KeyValuePair<TName, TItem>>
{
    /// <summary>
    /// Adds an item to the collection. The item name must be unique.
    /// </summary>
    /// <param name="itemName">The item name used as a key.</param>
    /// <param name="item">The item.</param>
    public void AddItem(TName itemName, TItem item)
    {
        Table.Add(itemName, item);
    }

    /// <summary>
    /// Checks whether the collection contains a given item.
    /// </summary>
    /// <param name="itemName">The item name.</param>
    /// <returns><see langword="true"/> if the collection contains an item with this name; otherwise, <see langword="false"/>.</returns>
    public bool ContainsItem(TName itemName)
    {
        return Table.ContainsKey(itemName);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TName, TItem>> GetEnumerator() => Table.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private Dictionary<TName, TItem> Table = new();
}
