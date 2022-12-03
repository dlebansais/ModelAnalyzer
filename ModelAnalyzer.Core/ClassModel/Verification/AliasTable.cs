namespace DemoAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a table of aliases.
/// </summary>
internal class AliasTable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AliasTable"/> class.
    /// </summary>
    public AliasTable()
    {
        Table = new();
        AllAliases = new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AliasTable"/> class.
    /// </summary>
    /// <param name="table">The table to clone.</param>
    /// <param name="allAliases">The aliases to clone.</param>
    private AliasTable(Dictionary<string, AliasName> table, List<string> allAliases)
        : this()
    {
        foreach (KeyValuePair<string, AliasName> Entry in table)
            Table.Add(Entry.Key, Entry.Value with { });

        AllAliases.AddRange(allAliases);
    }

    /// <summary>
    /// Adds a variable name to the table and creates its first alias.
    /// </summary>
    /// <param name="name">The name.</param>
    public void AddName(string name)
    {
        Debug.Assert(!Table.ContainsKey(name));

        Table.Add(name, new AliasName { VariableName = name });
        AllAliases.Add(Table[name].Alias);
    }

    /// <summary>
    /// Gets the current alias of a name.
    /// </summary>
    /// <param name="name">The name.</param>
    public string GetAlias(string name)
    {
        Debug.Assert(Table.ContainsKey(name));

        return Table[name].Alias;
    }

    /// <summary>
    /// Increments the name alias.
    /// </summary>
    /// <param name="name">The name.</param>
    public void IncrementNameAlias(string name)
    {
        Debug.Assert(Table.ContainsKey(name));

        Table[name].Increment();
        AllAliases.Add(Table[name].Alias);
    }

    /// <summary>
    /// Returns a clone of this instance.
    /// </summary>
    public AliasTable Clone()
    {
        return new AliasTable(Table, AllAliases);
    }

    /// <summary>
    /// Gets a list of aliases that this instance and the other don't have in common.
    /// </summary>
    /// <param name="other">The other instance.</param>
    public List<string> GetAliasDifference(AliasTable other)
    {
        List<string> AliasDifference = new();

        foreach (string Alias in AllAliases)
        {
            if (!other.AllAliases.Contains(Alias))
            {
                AliasDifference.Add(Alias);
            }
        }

        return AliasDifference;
    }

    /// <summary>
    /// Merges two instances.
    /// </summary>
    /// <param name="other">The other instance.</param>
    /// <param name="updatedNameList">The list of variables for which the alias has been incremented as a result of the merge.</param>
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

    private Dictionary<string, AliasName> Table;
    private List<string> AllAliases;
}
