namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents the location of a method or function call in a list of statements.
/// </summary>
internal class CallStatementLocation : ICallLocation
{
    /// <summary>
    /// Gets the parent block.
    /// </summary>
    required public BlockScope ParentBlock { get; init; }

    /// <summary>
    /// Gets the index of the statement in the list.
    /// </summary>
    required public int StatementIndex { get; init; }

    /// <inheritdoc/>
    public void RemoveCall()
    {
        Debug.Assert(StatementIndex >= 0);
        Debug.Assert(StatementIndex < ParentBlock.StatementList.Count);

        ParentBlock.StatementList.RemoveAt(StatementIndex);
    }
}
