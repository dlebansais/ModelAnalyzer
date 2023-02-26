namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a method local variable.
/// </summary>
[DebuggerDisplay("{Name.Text}")]
internal class Local : CodeVariable, ILocal, INameable<LocalName>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Local"/> class.
    /// </summary>
    /// <param name="name">The local name.</param>
    /// <param name="type">The local type.</param>
    public Local(LocalName name, ExpressionType type)
        : base(name, type)
    {
    }

    /// <inheritdoc/>
    IVariableName IVariable.Name { get => Name; }

    /// <inheritdoc/>
    LocalName INameable<LocalName>.Name { get => (LocalName)Name; }

    /// <inheritdoc/>
    required public ILiteralExpression? Initializer { get; init; }

    /// <inheritdoc/>
    required public MethodName MethodName { get; init; }
}
