namespace DemoAnalyzer;

using System.Collections.Generic;

internal class FieldTable : IEnumerable<KeyValuePair<IFieldName, IField>>
{
    private Dictionary<IFieldName, IField> Table = new();

    public void AddField(IFieldName fieldName, IField field)
    {
        Table.Add(fieldName, field);
    }

    public bool ContainsField(FieldName fieldName)
    {
        return Table.ContainsKey(fieldName);
    }

    public IEnumerator<KeyValuePair<IFieldName, IField>> GetEnumerator() => Table.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
