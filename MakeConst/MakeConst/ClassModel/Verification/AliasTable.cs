namespace DemoAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

public class AliasTable
{
    private Dictionary<string, AliasName> Table = new();

    public void AddName(string name)
    {
        Debug.Assert(!Table.ContainsKey(name));

        Table.Add(name, new AliasName { BaseName = name });
    }

    public string GetAlias(string name)
    {
        Debug.Assert(Table.ContainsKey(name));

        return Table[name].Alias;
    }

    public void IncrementNameAlias(string name)
    {
        Debug.Assert(Table.ContainsKey(name));

        Table[name].Increment();
    }
}
