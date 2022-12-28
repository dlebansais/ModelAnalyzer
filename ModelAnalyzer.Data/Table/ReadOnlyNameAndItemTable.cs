namespace ModelAnalyzer;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a read-only collection of items and name keys.
/// </summary>
/// <typeparam name="TName">The name type.</typeparam>
/// <typeparam name="TItem">The item type.</typeparam>
internal class ReadOnlyNameAndItemTable<TName, TItem>
    where TName : IEquatable<TName>
    where TItem : INameable<TName>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyNameAndItemTable{TName, TItem}"/> class.
    /// </summary>
    public ReadOnlyNameAndItemTable()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyNameAndItemTable{TName, TItem}"/> class.
    /// </summary>
    /// <param name="table">The source table.</param>
    public ReadOnlyNameAndItemTable(NameAndItemTable<TName, TItem> table)
    {
        foreach (KeyValuePair<TName, TItem> Entry in table)
            List.Add(Entry);
    }

    /// <summary>
    /// Checks whether the collection contains a given item.
    /// </summary>
    /// <param name="itemName">The item name.</param>
    /// <returns><see langword="true"/> if the collection contains an item with this name; otherwise, <see langword="false"/>.</returns>
    public bool ContainsItem(TName itemName)
    {
        return List.Exists((KeyValuePair<TName, TItem> entry) => entry.Key is TName Key && Key.Equals(itemName));
    }

    /// <summary>
    /// Enumerates the collection.
    /// </summary>
    public IEnumerator<KeyValuePair<TName, TItem>> GetEnumerator() => List.GetEnumerator();

    public List<KeyValuePair<TName, TItem>> List { get; set; } = new();
}
