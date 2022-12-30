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
    private AliasTable(Dictionary<Variable, VariableAlias> table, List<VariableAlias> allAliases)
        : this()
    {
        foreach (KeyValuePair<Variable, VariableAlias> Entry in table)
            Table.Add(Entry.Key, Entry.Value);

        AllAliases.AddRange(allAliases);
    }

    /// <summary>
    /// Checks whether a variable is in the table.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public bool ContainsVariable(Variable variable)
    {
        return Table.ContainsKey(variable);
    }

    /// <summary>
    /// Adds a variable to the table and creates its first alias.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public void AddVariable(Variable variable)
    {
        Debug.Assert(!ContainsVariable(variable));

        VariableAlias NewAlias = new VariableAlias(variable);

        Table.Add(variable, NewAlias);
        AllAliases.Add(NewAlias);
    }

    /// <summary>
    /// Adds a variable to the table and creates its first alias.
    /// If the variable already exists, increment the alias.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public void AddOrIncrement(Variable variable)
    {
        VariableAlias NewAlias;

        if (Table.ContainsKey(variable))
        {
            VariableAlias OldAlias = Table[variable];

            AllAliases.Remove(OldAlias);
            NewAlias = OldAlias.Incremented();
        }
        else
            NewAlias = new VariableAlias(variable);

        Table[variable] = NewAlias;
        AllAliases.Add(NewAlias);
    }

    /// <summary>
    /// Gets the current alias of a variable.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public VariableAlias GetAlias(Variable variable)
    {
        Debug.Assert(ContainsVariable(variable));

        return Table[variable];
    }

    /// <summary>
    /// Increments the variable alias.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public void IncrementAlias(Variable variable)
    {
        Debug.Assert(ContainsVariable(variable));

        VariableAlias OldAlias = Table[variable];
        VariableAlias NewAlias = OldAlias.Incremented();

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
    public List<VariableAlias> GetAliasDifference(AliasTable other)
    {
        List<VariableAlias> AliasDifference = new();

        foreach (VariableAlias Alias in AllAliases)
            if (!other.AllAliases.Contains(Alias))
                AliasDifference.Add(Alias);

        return AliasDifference;
    }

    /// <summary>
    /// Merges two instances.
    /// </summary>
    /// <param name="other">The other instance.</param>
    /// <param name="updatedFieldList">The list of variables for which the alias has been incremented as a result of the merge.</param>
    public void Merge(AliasTable other, out List<Variable> updatedFieldList)
    {
        updatedFieldList = new();
        Dictionary<Variable, VariableAlias> UpdatedTable = new();

        foreach (KeyValuePair<Variable, VariableAlias> Entry in Table)
        {
            Variable Variable = Entry.Key;
            VariableAlias OtherNameAlias = other.Table[Variable];
            VariableAlias MergedAlias = Entry.Value.Merged(OtherNameAlias, out bool IsUpdated);

            if (IsUpdated)
            {
                updatedFieldList.Add(Variable);
                UpdatedTable.Add(Variable, MergedAlias);
                AllAliases.Add(MergedAlias);
            }
        }

        foreach (KeyValuePair<Variable, VariableAlias> Entry in UpdatedTable)
            Table[Entry.Key] = Entry.Value;
    }

    private Dictionary<Variable, VariableAlias> Table;
    private List<VariableAlias> AllAliases;
}
