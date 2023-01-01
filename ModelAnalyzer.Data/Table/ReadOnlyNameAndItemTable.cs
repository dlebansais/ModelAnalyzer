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
        Table = new NameAndItemTable<TName, TItem>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyNameAndItemTable{TName, TItem}"/> class.
    /// </summary>
    /// <param name="table">The source table.</param>
    public ReadOnlyNameAndItemTable(NameAndItemTable<TName, TItem> table)
    {
        Table = table;
    }

    /// <summary>
    /// Gets a value indicating whether the table is empty.
    /// </summary>
    public bool IsEmpty { get => Table.List.Count == 0; }

    /// <summary>
    /// Gets the count of items in the table.
    /// </summary>
    public int Count { get => Table.List.Count; }

    /// <summary>
    /// Checks whether the collection contains a given item.
    /// </summary>
    /// <param name="itemName">The item name.</param>
    /// <returns><see langword="true"/> if the collection contains an item with this name; otherwise, <see langword="false"/>.</returns>
    public bool ContainsItem(TName itemName)
    {
        return Table.List.Exists((KeyValuePair<TName, TItem> entry) => entry.Key is TName Key && Key.Equals(itemName));
    }

    /// <summary>
    /// Enumerates the collection.
    /// </summary>
    public IEnumerator<KeyValuePair<TName, TItem>> GetEnumerator() => Table.List.GetEnumerator();

    public NameAndItemTable<TName, TItem> Table { get; set; }
}
