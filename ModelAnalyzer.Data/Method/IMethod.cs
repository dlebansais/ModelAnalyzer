namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides information about a method.
/// </summary>
public interface IMethod : INameable<MethodName>
{
    /// <summary>
    /// Gets the list of require clauses.
    /// </summary>
    IReadOnlyList<IRequire> GetRequires();

    /// <summary>
    /// Gets the list of ensure clauses.
    /// </summary>
    IReadOnlyList<IEnsure> GetEnsures();
}
