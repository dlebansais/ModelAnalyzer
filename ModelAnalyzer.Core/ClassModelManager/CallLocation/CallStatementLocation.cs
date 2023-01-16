namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents the location of a method or function call in a list of statements.
/// </summary>
internal class CallStatementLocation : ICallLocation
{
    /// <summary>
    /// Gets the list of statements.
    /// </summary>
    required public List<Statement> ParentStatementList { get; init; }

    /// <summary>
    /// Gets the index of the statement in the list.
    /// </summary>
    required public int StatementIndex { get; init; }

    /// <inheritdoc/>
    public void RemoveCall()
    {
        Debug.Assert(StatementIndex >= 0);
        Debug.Assert(StatementIndex < ParentStatementList.Count);

        ParentStatementList.RemoveAt(StatementIndex);
    }
}
