namespace ModelAnalyzer;

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
    private AliasTable(Dictionary<string, AliasName> table, List<AliasName> allAliases)
        : this()
    {
        foreach (KeyValuePair<string, AliasName> Entry in table)
            Table.Add(Entry.Key, Entry.Value with { });

        AllAliases.AddRange(allAliases);
    }

    /// <summary>
    /// Checks whether a variable name is in the table.
    /// </summary>
    /// <param name="name">The name.</param>
    public bool ContainsName(string name)
    {
        return Table.ContainsKey(name);
    }

    /// <summary>
    /// Adds a variable name to the table and creates its first alias.
    /// </summary>
    /// <param name="name">The name.</param>
    public void AddName(string name)
    {
        Debug.Assert(!ContainsName(name));

        AliasName NewAlias = new AliasName(name);

        Table.Add(name, NewAlias);
        AllAliases.Add(NewAlias);
    }

    /// <summary>
    /// Adds a variable name to the table and creates its first alias.
    /// If the name already exists, increment the alias.
    /// </summary>
    /// <param name="name">The name.</param>
    public void AddOrIncrementName(string name)
    {
        AliasName NewAlias;

        if (Table.ContainsKey(name))
        {
            AliasName OldAlias = Table[name];

            AllAliases.Remove(OldAlias);
            NewAlias = OldAlias.Incremented();
        }
        else
            NewAlias = new AliasName(name);

        Table[name] = NewAlias;
        AllAliases.Add(NewAlias);
    }

    /// <summary>
    /// Gets the current alias of a name.
    /// </summary>
    /// <param name="name">The name.</param>
    public AliasName GetAlias(string name)
    {
        Debug.Assert(ContainsName(name));

        return Table[name];
    }

    /// <summary>
    /// Increments the name alias.
    /// </summary>
    /// <param name="name">The name.</param>
    public void IncrementNameAlias(string name)
    {
        Debug.Assert(ContainsName(name));

        AliasName OldAlias = Table[name];
        AliasName NewAlias = OldAlias.Incremented();

        Table[name] = NewAlias;
        AllAliases.Add(NewAlias);
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
    public List<AliasName> GetAliasDifference(AliasTable other)
    {
        List<AliasName> AliasDifference = new();

        foreach (AliasName Alias in AllAliases)
            if (!other.AllAliases.Contains(Alias))
                AliasDifference.Add(Alias);

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
        Dictionary<string, AliasName> UpdatedTable = new();

        foreach (KeyValuePair<string, AliasName> Entry in Table)
        {
            string Name = Entry.Key;
            AliasName OtherNameAlias = other.Table[Name];
            AliasName MergedAlias = Entry.Value.Merged(OtherNameAlias, out bool IsUpdated);

            if (IsUpdated)
            {
                updatedNameList.Add(Name);
                UpdatedTable.Add(Name, MergedAlias);
                AllAliases.Add(MergedAlias);
            }
        }

        foreach (KeyValuePair<string, AliasName> Entry in UpdatedTable)
            Table[Entry.Key] = Entry.Value;
    }

    private Dictionary<string, AliasName> Table;
    private List<AliasName> AllAliases;
}
