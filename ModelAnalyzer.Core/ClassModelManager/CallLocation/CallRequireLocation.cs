namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents the location of a method or function call in a list of require clauses.
/// </summary>
internal class CallRequireLocation : ICallLocation
{
    /// <summary>
    /// Gets the list of require clauses.
    /// </summary>
    required public List<Require> ParentRequireList { get; init; }

    /// <summary>
    /// Gets the index of the clause in the list.
    /// </summary>
    required public int RequireIndex { get; init; }

    /// <inheritdoc/>
    public void RemoveCall()
    {
        Debug.Assert(RequireIndex >= 0);
        Debug.Assert(RequireIndex < ParentRequireList.Count);

        ParentRequireList.RemoveAt(RequireIndex);
    }
}
