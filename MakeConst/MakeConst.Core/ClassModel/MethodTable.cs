namespace DemoAnalyzer;

using System.Collections.Generic;

public class MethodTable : IEnumerable<KeyValuePair<MethodName, IMethod>>
{
    private Dictionary<MethodName, IMethod> Table = new();

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
