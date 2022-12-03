namespace DemoAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents a collection of fields and name keys.
/// </summary>
internal class FieldTable : IEnumerable<KeyValuePair<FieldName, IField>>
{
    /// <summary>
    /// Adds a field to the collection. The field name must be unique.
    /// </summary>
    /// <param name="fieldName">The field name used as a key.</param>
    /// <param name="field">The field.</param>
    public void AddField(FieldName fieldName, IField field)
    {
        Table.Add(fieldName, field);
    }

    /// <summary>
    /// Checks whether the collection contains a given field.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <returns><see langword="True"/> if the collection contains a field with this name; otherwise, <see langword="False"/>.</returns>
    public bool ContainsField(FieldName fieldName)
    {
        return Table.ContainsKey(fieldName);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<FieldName, IField>> GetEnumerator() => Table.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private Dictionary<FieldName, IField> Table = new();
}
