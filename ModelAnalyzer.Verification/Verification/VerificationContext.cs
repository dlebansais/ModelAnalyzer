namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents the context to use when verifying a class.
/// </summary>
internal record VerificationContext : IMemberCollectionContext
{
    /// <summary>
    /// Gets or sets the table of class fields.
    /// </summary>
    public ReadOnlyFieldTable FieldTable { get; set; } = new();

    /// <inheritdoc/>
    List<Field> IMemberCollectionContext.GetFields()
    {
        List<Field> Result = new();

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
            Result.Add(Entry.Value);

        return Result;
    }

    /// <summary>
    /// Gets or sets the table of class methods.
    /// </summary>
    public ReadOnlyMethodTable MethodTable { get; set; } = new();

    /// <inheritdoc/>
    List<Method> IMemberCollectionContext.GetMethods()
    {
        List<Method> Result = new();

        foreach (KeyValuePair<MethodName, Method> Entry in MethodTable)
            Result.Add(Entry.Value);

        return Result;
    }

    /// <summary>
    /// Gets or sets the method within which parsing is taking place. This is null when parsing fields or invariant clauses for instance.
    /// </summary>
    public Method? HostMethod { get; set; }

    /// <summary>
    /// Gets or sets the local variable that represents the value returned by a method. This is either a local declared in the method or one made up by the parser.
    /// </summary>
    public Local? ResultLocal { get; set; }

    /// <summary>
    /// Gets or sets the table of aliases.
    /// </summary>
    public AliasTable AliasTable { get; set; } = new();
}
