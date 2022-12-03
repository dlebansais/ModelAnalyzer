namespace DemoAnalyzer;

using System.Collections.Generic;

internal class MethodTable : IEnumerable<KeyValuePair<IMethodName, IMethod>>
{
    private Dictionary<IMethodName, IMethod> Table = new();

    public void AddMethod(IMethodName fieldName, IMethod field)
    {
        Table.Add(fieldName, field);
    }

    public bool ContainsMethod(IMethodName fieldName)
    {
        return Table.ContainsKey(fieldName);
    }

    public IEnumerator<KeyValuePair<IMethodName, IMethod>> GetEnumerator() => Table.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
