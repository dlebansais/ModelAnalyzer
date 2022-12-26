﻿namespace ModelAnalyzer;

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
    private AliasTable(Dictionary<IVariable, AliasName> table, List<AliasName> allAliases)
        : this()
    {
        foreach (KeyValuePair<IVariable, AliasName> Entry in table)
            Table.Add(Entry.Key, Entry.Value);

        AllAliases.AddRange(allAliases);
    }

    /// <summary>
    /// Checks whether a variable is in the table.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public bool ContainsVariable(IVariable variable)
    {
        return Table.ContainsKey(variable);
    }

    /// <summary>
    /// Adds a variable to the table and creates its first alias.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public void AddVariable(IVariable variable)
    {
        Debug.Assert(!ContainsVariable(variable));

        AliasName NewAlias = new AliasName(variable);

        Table.Add(variable, NewAlias);
        AllAliases.Add(NewAlias);
    }

    /// <summary>
    /// Adds a variable to the table and creates its first alias.
    /// If the variable already exists, increment the alias.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public void AddOrIncrement(IVariable variable)
    {
        AliasName NewAlias;

        if (Table.ContainsKey(variable))
        {
            AliasName OldAlias = Table[variable];

            AllAliases.Remove(OldAlias);
            NewAlias = OldAlias.Incremented();
        }
        else
            NewAlias = new AliasName(variable);

        Table[variable] = NewAlias;
        AllAliases.Add(NewAlias);
    }

    /// <summary>
    /// Gets the current alias of a variable.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public AliasName GetAlias(IVariable variable)
    {
        Debug.Assert(ContainsVariable(variable));

        return Table[variable];
    }

    /// <summary>
    /// Increments the variable alias.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public void IncrementNameAlias(IVariable variable)
    {
        Debug.Assert(ContainsVariable(variable));

        AliasName OldAlias = Table[variable];
        AliasName NewAlias = OldAlias.Incremented();

        Table[variable] = NewAlias;
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
    /// <param name="updatedFieldList">The list of variables for which the alias has been incremented as a result of the merge.</param>
    public void Merge(AliasTable other, out List<IVariable> updatedFieldList)
    {
        updatedFieldList = new();
        Dictionary<IVariable, AliasName> UpdatedTable = new();

        foreach (KeyValuePair<IVariable, AliasName> Entry in Table)
        {
            IVariable Variable = Entry.Key;
            AliasName OtherNameAlias = other.Table[Variable];
            AliasName MergedAlias = Entry.Value.Merged(OtherNameAlias, out bool IsUpdated);

            if (IsUpdated)
            {
                updatedFieldList.Add(Variable);
                UpdatedTable.Add(Variable, MergedAlias);
                AllAliases.Add(MergedAlias);
            }
        }

        foreach (KeyValuePair<IVariable, AliasName> Entry in UpdatedTable)
            Table[Entry.Key] = Entry.Value;
    }

    private Dictionary<IVariable, AliasName> Table;
    private List<AliasName> AllAliases;
}
