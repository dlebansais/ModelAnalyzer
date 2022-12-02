namespace DemoAnalyzer;

using System.Collections.Generic;

public class FieldTable : IEnumerable<KeyValuePair<FieldName, IField>>
{
    private Dictionary<FieldName, IField> Table = new();

    public void AddField(FieldName fieldName, IField field)
    {
        Table.Add(fieldName, field);
    }

    public bool ContainsField(FieldName fieldName)
    {
        return Table.ContainsKey(fieldName);
    }

    public IEnumerator<KeyValuePair<FieldName, IField>> GetEnumerator() => Table.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
