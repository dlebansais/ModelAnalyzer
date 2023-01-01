namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents the context of a parsed method call statement.
/// </summary>
internal class MethodCallStatementEntry
{
    /// <summary>
    /// Gets the parsed statement.
    /// </summary>
    required public MethodCallStatement Statement { get; init; }

    /// <summary>
    /// Gets the host method where the statement is found.
    /// </summary>
    required public Method HostMethod { get; init; }

    /// <summary>
    /// Gets the parent statement list.
    /// </summary>
    required public List<Statement> ParentStatementList { get; init; }
}
