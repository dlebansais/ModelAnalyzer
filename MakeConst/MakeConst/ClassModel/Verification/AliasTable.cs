namespace DemoAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

public class AliasTable
{
    public AliasTable()
    {
        Table = new();
        AllAliases = new();
    }

    private AliasTable(Dictionary<string, AliasName> table, List<string> allAliases)
        : this()
    {
        foreach (KeyValuePair<string, AliasName> Entry in table)
            Table.Add(Entry.Key, Entry.Value with { });

        AllAliases.AddRange(allAliases);
    }

    private Dictionary<string, AliasName> Table;
    private List<string> AllAliases;

    public void AddName(string name)
    {
        Debug.Assert(!Table.ContainsKey(name));

        Table.Add(name, new AliasName { BaseName = name });
        AllAliases.Add(Table[name].Alias);
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
        AllAliases.Add(Table[name].Alias);
    }

    public AliasTable Clone()
    {
        return new AliasTable(Table, AllAliases);
    }

    public List<string> GetAliasDifference(AliasTable other)
    {
        List<string> AliasDifference = new();

        foreach (string Alias in AllAliases)
        {
            if (!other.AllAliases.Contains(Alias))
            {
                Logger.Log($"{Alias} is only in 1");
                AliasDifference.Add(Alias);
            }
        }

        return AliasDifference;
    }

    public void Merge(AliasTable other, out List<string> updatedNameList)
    {
        updatedNameList = new();

        foreach (KeyValuePair<string, AliasName> Entry in Table)
        {
            string Name = Entry.Key;
            AliasName OtherNameAlias = other.Table[Name];
            Entry.Value.Merge(OtherNameAlias, out bool IsUpdated);

            if (IsUpdated)
            {
                updatedNameList.Add(Name);
                AllAliases.Add(Table[Name].Alias);
            }
        }
    }
}
