namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a class method.
/// </summary>
[DebuggerDisplay("{Name.Text}()")]
internal class Method : IMethod
{
    /// <summary>
    /// Gets the method name.
    /// </summary>
    required public MethodName Name { get; init; }

    /// <summary>
    /// Gets the access modifier.
    /// </summary>
    required public AccessModifier AccessModifier { get; init; }

    /// <summary>
    /// Gets a value indicating whether the method returns a value.
    /// </summary>
    required public ExpressionType ReturnType { get; init; }

    /// <summary>
    /// Gets the method parameters.
    /// </summary>
    required public ReadOnlyParameterTable ParameterTable { get; init; }

    /// <summary>
    /// Gets the method requirements.
    /// </summary>
    required public List<Require> RequireList { get; init; }

    /// <summary>
    /// Gets the method local variables.
    /// </summary>
    required public ReadOnlyLocalTable LocalTable { get; init; }

    /// <summary>
    /// Gets the method statements.
    /// </summary>
    required public List<Statement> StatementList { get; init; }

    /// <summary>
    /// Gets the method guarantees.
    /// </summary>
    required public List<Ensure> EnsureList { get; init; }

    /// <inheritdoc/>
    public IList<IRequire> GetRequires() => new List<IRequire>(RequireList);

    /// <inheritdoc/>
    public IList<ILocal> GetLocals() => new List<ILocal>(LocalTable.Table.List.ConvertAll(entry => entry.Value));

    /// <inheritdoc/>
    public IList<IEnsure> GetEnsures() => new List<IEnsure>(EnsureList);
}
