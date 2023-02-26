namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a method parameter.
/// </summary>
[DebuggerDisplay("{Name.Text}")]
internal record Parameter : Variable, IParameter, INameable<ParameterName>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Parameter"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="type">The parameter type.</param>
    public Parameter(ParameterName name, ExpressionType type)
        : base(name, type)
    {
    }

    /// <inheritdoc/>
    IVariableName IVariable.Name { get => Name; }

    /// <inheritdoc/>
    ParameterName INameable<ParameterName>.Name { get => (ParameterName)Name; }

    /// <inheritdoc/>
    required public MethodName MethodName { get; init; }
}
