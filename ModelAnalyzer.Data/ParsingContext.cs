namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents the context to use when parsing a class.
/// </summary>
internal record ParsingContext
{
    /// <summary>
    /// Gets the object collecting all unsupported elements found during parsing.
    /// </summary>
    public Unsupported Unsupported { get; } = new();

    /// <summary>
    /// Gets or sets the table of class fields.
    /// </summary>
    public FieldTable FieldTable { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the first pass of method parsing is done.
    /// </summary>
    public bool IsMethodParsingFirstPassDone { get; set; }

    /// <summary>
    /// Gets or sets the table of class methods.
    /// </summary>
    public MethodTable MethodTable { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether all passes of method parsing are done.
    /// </summary>
    public bool IsMethodParsingComplete { get; set; }

    /// <summary>
    /// Gets or sets the method within which parsing is taking place. This is null when parsing fields or invariant clauses for instance.
    /// </summary>
    public Method? HostMethod { get; set; }

    /// <summary>
    /// Gets or sets the list of invariants.
    /// </summary>
    public List<Invariant> InvariantList { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether local variables are allowed when parsing expressions. This is false when parsing assertion clauses for instance.
    /// </summary>
    public bool IsLocalAllowed { get; set; }

    /// <summary>
    /// Gets or sets the local variable that represents the value returned by a method. This is either a local declared in the method or one made up by the parser.
    /// </summary>
    public Local? ResultLocal { get; set; }

    /// <summary>
    /// Gets or sets a context to calculate the location of an expression. This is usually relative to the source code root, but also can be relative to a trivia for assertions, for instance.
    /// </summary>
    public LocationContext? LocationContext { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether parsing is taking place inside an expression. This is mainly to avoid logging pieces of an invalid expression, we just want to log the entire expression after it's parsed.
    /// </summary>
    public bool IsExpressionNested { get; set; }
}
