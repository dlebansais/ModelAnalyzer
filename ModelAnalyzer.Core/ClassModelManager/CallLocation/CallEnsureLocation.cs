namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents the location of a method or function call in a list of ensure clauses.
/// </summary>
internal class CallEnsureLocation : ICallLocation
{
    /// <summary>
    /// Gets the list of ensure clauses.
    /// </summary>
    required public List<Ensure> ParentEnsureList { get; init; }

    /// <summary>
    /// Gets the index of the clause in the list.
    /// </summary>
    required public int EnsureIndex { get; init; }

    /// <inheritdoc/>
    public void RemoveCall()
    {
        Debug.Assert(EnsureIndex >= 0);
        Debug.Assert(EnsureIndex < ParentEnsureList.Count);

        ParentEnsureList.RemoveAt(EnsureIndex);
    }
}
