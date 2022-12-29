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
    /// Gets the method statements.
    /// </summary>
    required public List<Statement> StatementList { get; init; }

    /// <summary>
    /// Gets the method guarantees.
    /// </summary>
    required public List<Ensure> EnsureList { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<IRequire> GetRequires()
    {
        List<IRequire> Result = new();

        foreach (Require Item in RequireList)
            Result.Add(Item);

        return Result.AsReadOnly();
    }

    /// <inheritdoc/>
    public IReadOnlyList<IEnsure> GetEnsures()
    {
        List<IEnsure> Result = new();

        foreach (Ensure Item in EnsureList)
            Result.Add(Item);

        return Result.AsReadOnly();
    }
}
