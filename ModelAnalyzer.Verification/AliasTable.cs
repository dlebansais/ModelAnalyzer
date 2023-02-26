namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

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
    private AliasTable(Dictionary<string, VariableAlias> table, List<VariableAlias> allAliases)
        : this()
    {
        foreach (KeyValuePair<string, VariableAlias> Entry in table)
            Table.Add(Entry.Key, Entry.Value);

        AllAliases.AddRange(allAliases);
    }

    /// <summary>
    /// Checks whether a variable is in the table.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public bool ContainsVariable(Variable variable)
    {
        return Table.ContainsKey(variable.Name.Text);
    }

    /// <summary>
    /// Adds a variable to the table and creates its first alias.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public void AddVariable(Variable variable)
    {
        Debug.Assert(!ContainsVariable(variable));

        VariableAlias NewAlias = CreateAlias(variable);

        // Base alias => base variable
        Debug.Assert(NewAlias.GetType().Name != typeof(VariableAlias).Name || variable.GetType().Name == typeof(Variable).Name);

        Table.Add(variable.Name.Text, NewAlias);
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
        string Key = variable.Name.Text;

        if (Table.ContainsKey(Key))
        {
            VariableAlias OldAlias = Table[Key];

            AllAliases.Remove(OldAlias);
            NewAlias = OldAlias.Incremented();

            Table[Key] = NewAlias;
            AllAliases.Add(NewAlias);
        }
        else
        {
            NewAlias = CreateAlias(variable);

            // Base alias => base variable
            Debug.Assert(NewAlias.GetType().Name != typeof(VariableAlias).Name || variable.GetType().Name == typeof(Variable).Name);

            Table.Add(Key, NewAlias);
            AllAliases.Add(NewAlias);
        }
    }

    private VariableAlias CreateAlias(Variable variable)
    {
        switch (variable)
        {
            case Field AsField:
                return new FieldAlias(AsField);
            case Property AsProperty:
                return new PropertyAlias(AsProperty);
            case Parameter AsParameter:
                return new ParameterAlias(AsParameter);
            case Local AsLocal:
                return new LocalAlias(AsLocal);
            default:
                return new VariableAlias(variable);
        }
    }

    /// <summary>
    /// Gets the current alias of a variable.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public VariableAlias GetAlias(Variable variable)
    {
        string Key = variable.Name.Text;

        Debug.Assert(Table.ContainsKey(Key));

        return Table[Key];
    }

    /// <summary>
    /// Increments the variable alias.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public void IncrementAlias(Variable variable)
    {
        string Key = variable.Name.Text;

        Debug.Assert(Table.ContainsKey(Key));

        VariableAlias OldAlias = Table[Key];
        VariableAlias NewAlias = OldAlias.Incremented();

        Table[Key] = NewAlias;
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
    /// <param name="methodName">The method where the marge is taking place.</param>
    /// <param name="other">The other instance.</param>
    /// <param name="updatedVariableList">The list of variables for which the alias has been incremented as a result of the merge.</param>
    public void Merge(MethodName methodName, AliasTable other, out List<Variable> updatedVariableList)
    {
        updatedVariableList = new();
        Dictionary<string, VariableAlias> UpdatedTable = new();
        List<string> KeyList = new();

        foreach (KeyValuePair<string, VariableAlias> Entry in Table)
            if (other.Table.ContainsKey(Entry.Key))
                KeyList.Add(Entry.Key);

        foreach (string Key in KeyList)
        {
            Debug.Assert(other.Table.ContainsKey(Key));

            Variable Variable = Table[Key].Variable;
            VariableAlias OtherNameAlias = other.Table[Key];
            VariableAlias MergedAlias = Table[Key].Merged(OtherNameAlias, out bool IsUpdated);

            if (IsUpdated)
            {
                updatedVariableList.Add(Variable);
                UpdatedTable.Add(Key, MergedAlias);
                AllAliases.Add(MergedAlias);
            }
        }

        foreach (KeyValuePair<string, VariableAlias> Entry in UpdatedTable)
            Table[Entry.Key] = Entry.Value;
    }

    private Dictionary<string, VariableAlias> Table;
    private List<VariableAlias> AllAliases;
}
