﻿namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents the context to use when parsing a class.
/// </summary>
internal record ParsingContext
{
    /// <summary>
    /// Gets the list of class declarations.
    /// </summary>
    required public List<ClassDeclarationSyntax> ClassDeclarationList { get; init; }

    /// <summary>
    /// Gets the name of the parsed class.
    /// </summary>
    required public ClassName ClassName { get; init; }

    /// <summary>
    /// Gets the semantic model.
    /// </summary>
    required public IModel SemanticModel { get; init; }

    /// <summary>
    /// Gets the object collecting all unsupported elements found during parsing.
    /// </summary>
    public Unsupported Unsupported { get; } = new();

    /// <summary>
    /// Gets or sets the table of class properties.
    /// </summary>
    public PropertyTable PropertyTable { get; set; } = new();

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
    /// Gets or sets the method within which parsing is taking place. This is null when parsing properties, fields or invariant clauses for instance.
    /// </summary>
    public Method? HostMethod { get; set; }

    /// <summary>
    /// Gets or sets the block within which parsing is taking place. This is null when parsing properties, fields or invariant clauses for instance.
    /// </summary>
    public BlockScope? HostBlock { get; set; }

    /// <summary>
    /// Gets or sets the list of invariants.
    /// </summary>
    public List<Invariant> InvariantList { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether fields are allowed when parsing expressions. This is false when parsing require or ensure clauses for instance.
    /// </summary>
    public bool IsFieldAllowed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether local variables are allowed when parsing expressions. This is false when parsing assertion clauses for instance.
    /// </summary>
    public bool IsLocalAllowed { get; set; }

    /// <summary>
    /// Gets or sets a context to calculate the location of an expression. This is usually relative to the source code root, but also can be relative to a trivia for assertions, for instance.
    /// </summary>
    public LocationContext? LocationContext { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether parsing is taking place inside an expression. This is mainly to avoid logging pieces of an invalid expression, we just want to log the entire expression after it's parsed.
    /// </summary>
    public bool IsExpressionNested { get; set; }

    /// <summary>
    /// Gets or sets the current call location.
    /// </summary>
    public ICallLocation? CallLocation { get; set; }

    /// <summary>
    /// Gets or sets the list of method call statements.
    /// </summary>
    public List<MethodCallStatementEntry> MethodCallStatementList { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of function call expressions.
    /// </summary>
    public List<FunctionCallExpressionEntry> FunctionCallExpressionList { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of arithmetic expressions.
    /// </summary>
    public List<ArithmeticExpressionEntry> ArithmeticExpressionList { get; set; } = new();
}
