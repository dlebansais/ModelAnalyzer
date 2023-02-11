namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a class method.
/// </summary>
[DebuggerDisplay("{Name.Text, nq}()")]
internal class Method : IMethod, INameable<MethodName>
{
    /// <summary>
    /// Gets the method name.
    /// </summary>
    required public MethodName Name { get; init; }

    /// <inheritdoc/>
    IClassMemberName IMethod.Name { get => Name; }

    /// <summary>
    /// Gets the name of the class containing this method.
    /// </summary>
    required public ClassName ClassName { get; init; }

    /// <summary>
    /// Gets the access modifier.
    /// </summary>
    required public AccessModifier AccessModifier { get; init; }

    /// <summary>
    /// Gets a value indicating whether the method is static.
    /// </summary>
    required public bool IsStatic { get; init; }

    /// <summary>
    /// Gets a value indicating whether the method is preloaded.
    /// </summary>
    required public bool IsPreloaded { get; init; }

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
    /// Gets the method root block.
    /// </summary>
    required public BlockScope RootBlock { get; init; }

    /// <summary>
    /// Gets or sets the Result local variable.
    /// </summary>
    public ILocal? ResultLocal { get; set; }

    /// <summary>
    /// Gets the method guarantees.
    /// </summary>
    required public List<Ensure> EnsureList { get; init; }

    /// <inheritdoc/>
    public IList<IParameter> GetParameters() => new List<IParameter>(ParameterTable.Table.List.ConvertAll(entry => entry.Value));

    /// <inheritdoc/>
    public IList<IRequire> GetRequires() => new List<IRequire>(RequireList);

    /// <inheritdoc/>
    public IList<ILocal> GetLocals() => new List<ILocal>(RootBlock.LocalTable.Table.List.ConvertAll(entry => entry.Value));

    /// <inheritdoc/>
    public IList<IEnsure> GetEnsures() => new List<IEnsure>(EnsureList);
}
