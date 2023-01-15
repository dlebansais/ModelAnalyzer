namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides information about a method.
/// </summary>
public interface IMethod
{
    /// <summary>
    /// Gets the method name.
    /// </summary>
    IClassMemberName Name { get; }

    /// <summary>
    /// Gets the return type.
    /// </summary>
    ExpressionType ReturnType { get; }

    /// <summary>
    /// Gets the list of parameters.
    /// </summary>
    IList<IParameter> GetParameters();

    /// <summary>
    /// Gets the list of require clauses.
    /// </summary>
    IList<IRequire> GetRequires();

    /// <summary>
    /// Gets the list of local variables.
    /// </summary>
    IList<ILocal> GetLocals();

    /// <summary>
    /// Gets the Result local variable.
    /// </summary>
    ILocal? ResultLocal { get; }

    /// <summary>
    /// Gets the list of ensure clauses.
    /// </summary>
    IList<IEnsure> GetEnsures();
}
