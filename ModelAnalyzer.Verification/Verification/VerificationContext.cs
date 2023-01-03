namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.Z3;

/// <summary>
/// Represents the context to use when verifying a class.
/// </summary>
internal record VerificationContext : IMemberCollectionContext
{
    /// <summary>
    /// Gets the solver.
    /// </summary>
    required public Solver Solver { get; init; }

    /// <summary>
    /// Gets or sets the table of class properties.
    /// </summary>
    public ReadOnlyPropertyTable PropertyTable { get; set; } = new();

    /// <inheritdoc/>
    List<Property> IMemberCollectionContext.GetProperties()
    {
        List<Property> Result = new();

        foreach (KeyValuePair<PropertyName, Property> Entry in PropertyTable)
            Result.Add(Entry.Value);

        return Result;
    }

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
    /// Gets or sets the method within which parsing is taking place. This is null when parsing properties, fields or invariant clauses for instance.
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

    /// <summary>
    /// Gets or sets the execution branch. Null if not within some conditional statement.
    /// </summary>
    public BoolExpr? Branch { get; set; }
}
