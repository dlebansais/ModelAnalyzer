namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a class method.
/// </summary>
[DebuggerDisplay("{MethodName.Name}()")]
internal class Method : IMethod
{
    /// <summary>
    /// Gets the method name.
    /// </summary>
    required public MethodName MethodName { get; init; }

    /// <summary>
    /// Gets a value indicating whether all method members are supported.
    /// </summary>
    required public bool IsSupported { get; init; }

    /// <summary>
    /// Gets a value indicating whether the method returns a value.
    /// </summary>
    required public bool HasReturnValue { get; init; }

    /// <summary>
    /// Gets the method parameters.
    /// </summary>
    required public ParameterTable ParameterTable { get; init; }

    /// <summary>
    /// Gets the method requirements.
    /// </summary>
    required public List<IRequire> RequireList { get; init; }

    /// <summary>
    /// Gets the method statements.
    /// </summary>
    required public List<IStatement> StatementList { get; init; }

    /// <summary>
    /// Gets the method guarantees.
    /// </summary>
    required public List<IEnsure> EnsureList { get; init; }

    /// <inheritdoc/>
    public string Name { get { return MethodName.Name; } }
}
