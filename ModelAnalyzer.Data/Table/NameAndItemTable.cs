﻿namespace ModelAnalyzer;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a collection of items and name keys.
/// </summary>
/// <typeparam name="TName">The name type.</typeparam>
/// <typeparam name="TItem">The item type.</typeparam>
internal class NameAndItemTable<TName, TItem>
    where TName : IEquatable<TName>
{
    /// <summary>
    /// Gets or sets a value indicating whether the table is sealed.
    /// </summary>
    public bool IsSealed { get; set; }

    /// <summary>
    /// Adds an item to the collection. The item name must be unique, and the table not sealed.
    /// </summary>
    /// <param name="itemName">The item name used as a key.</param>
    /// <param name="item">The item.</param>
    public void AddItem(TName itemName, TItem item)
    {
        if (IsSealed)
            throw new InvalidOperationException("Cannot add items to a sealed table.");

        List.Add(new KeyValuePair<TName, TItem>(itemName, item));
    }

    /// <summary>
    /// Seals the table, forbiding subsequent calls to <see cref="AddItem(TName, TItem)"/>.
    /// </summary>
    public void Seal()
    {
        IsSealed = true;
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

    /// <summary>
    /// Gets or sets the table.
    /// </summary>
    public List<KeyValuePair<TName, TItem>> List { get; set; } = new();
}
