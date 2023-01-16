namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents the location of a method or function call in a list of invariants.
/// </summary>
internal class CallInvariantLocation : ICallLocation
{
    /// <summary>
    /// Gets the list of invariants.
    /// </summary>
    required public List<Invariant> ParentInvariantList { get; init; }

    /// <summary>
    /// Gets the index of the invariant in the list.
    /// </summary>
    required public int InvariantIndex { get; init; }

    /// <inheritdoc/>
    public void RemoveCall()
    {
        Debug.Assert(InvariantIndex >= 0 && InvariantIndex < ParentInvariantList.Count);

        ParentInvariantList.RemoveAt(InvariantIndex);
    }
}
