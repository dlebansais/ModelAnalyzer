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
    IList<IRequire> GetRequires();

    /// <summary>
    /// Gets the list of local variables.
    /// </summary>
    IList<ILocal> GetLocals();

    /// <summary>
    /// Gets the list of ensure clauses.
    /// </summary>
    IList<IEnsure> GetEnsures();
}
