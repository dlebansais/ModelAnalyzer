namespace DemoAnalyzer;

using System.Collections.Generic;

internal class ParameterTable : IEnumerable<KeyValuePair<ParameterName, IParameter>>
{
    private Dictionary<ParameterName, IParameter> Table = new();

    public void AddParameter(ParameterName fieldName, IParameter field)
    {
        Table.Add(fieldName, field);
    }

    public bool ContainsParameter(ParameterName fieldName)
    {
        return Table.ContainsKey(fieldName);
    }

    public IEnumerator<KeyValuePair<ParameterName, IParameter>> GetEnumerator() => Table.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
