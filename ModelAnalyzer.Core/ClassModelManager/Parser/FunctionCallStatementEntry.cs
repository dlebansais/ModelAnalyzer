namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents the context of a parsed function call expression.
/// </summary>
internal class FunctionCallStatementEntry
{
    /// <summary>
    /// Gets the parsed statement.
    /// </summary>
    required public FunctionCallExpression Expression { get; init; }

    /// <summary>
    /// Gets the host method where the statement is found.
    /// </summary>
    required public Method? HostMethod { get; init; }

    /// <summary>
    /// Gets the index within <see cref="ParentStatementList"/> of the statement where the expression is parsed. If -1, this is a method expression body.
    /// </summary>
    required public int OwnerStatementIndex { get; init; }

    /// <summary>
    /// Gets the parent statement list.
    /// </summary>
    required public List<Statement> ParentStatementList { get; init; }
}
