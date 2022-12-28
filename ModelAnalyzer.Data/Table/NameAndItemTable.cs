namespace ModelAnalyzer;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a collection of items and name keys.
/// </summary>
/// <typeparam name="TName">The name type.</typeparam>
/// <typeparam name="TItem">The item type.</typeparam>
internal class NameAndItemTable<TName, TItem>
    where TName : IEquatable<TName>
    where TItem : INameable<TName>
{
    /// <summary>
    /// Adds an item to the collection. The item name must be unique.
    /// </summary>
    /// <param name="item">The item.</param>
    public void AddItem(TItem item)
    {
        List.Add(new KeyValuePair<TName, TItem>(item.Name, item));
    }

    /// <summary>
    /// Checks whether the collection contains a given item.
    /// </summary>
    /// <param name="name">The item name.</param>
    /// <returns><see langword="true"/> if the collection contains an item with this name; otherwise, <see langword="false"/>.</returns>
    public bool ContainsItem(TName name)
    {
        return List.Exists((KeyValuePair<TName, TItem> entry) => entry.Key is TName Key && Key.Equals(name));
    }

    /// <summary>
    /// Enumerates the collection.
    /// </summary>
    public IEnumerator<KeyValuePair<TName, TItem>> GetEnumerator() => List.GetEnumerator();

    /// <summary>
    /// Gets or sets the table.
    /// </summary>
    public List<KeyValuePair<TName, TItem>> List { get; set; } = new();
}
