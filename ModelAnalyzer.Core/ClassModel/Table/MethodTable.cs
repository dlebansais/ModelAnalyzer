namespace DemoAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents a collection of methods and name keys.
/// </summary>
internal class MethodTable : IEnumerable<KeyValuePair<MethodName, IMethod>>
{
    private Dictionary<MethodName, IMethod> Table = new();

    /// <summary>
    /// Adds a method to the collection. The field name must be unique.
    /// </summary>
    /// <param name="fieldName">The field name used as a key.</param>
    /// <param name="field">The field.</param>
    public void AddMethod(MethodName fieldName, IMethod field)
    {
        Table.Add(fieldName, field);
    }

    public bool ContainsMethod(MethodName fieldName)
    {
        return Table.ContainsKey(fieldName);
    }

    public IEnumerator<KeyValuePair<MethodName, IMethod>> GetEnumerator() => Table.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
