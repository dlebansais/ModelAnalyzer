namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents a set of names.
/// </summary>
internal class NameSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NameSet"/> class.
    /// </summary>
    /// <param name="name">The name that makes the set.</param>
    public NameSet(string name)
    {
        Names = new List<string>() { name }.AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NameSet"/> class.
    /// </summary>
    /// <param name="name">The name of the type with properties.</param>
    /// <param name="propertyNames">The properties.</param>
    public NameSet(string name, ICollection<NameSet> propertyNames)
    {
        List<string> NameList = new() { name };

        foreach (NameSet Item in propertyNames)
            NameList.AddRange(Item.Names);

        Names = NameList.AsReadOnly();
    }

    /// <summary>
    /// Gets the list of names in the set.
    /// </summary>
    public IReadOnlyList<string> Names { get; }
}
