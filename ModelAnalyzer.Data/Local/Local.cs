namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a method local variable.
/// </summary>
[DebuggerDisplay("{Name.Text}")]
internal class Local : ILocal, INameable<LocalName>
{
    /// <summary>
    /// Gets the local name.
    /// </summary>
    required public LocalName Name { get; init; }

    /// <inheritdoc/>
    IVariableName IVariable.Name { get => Name; }

    /// <inheritdoc/>
    required public ExpressionType Type { get; init; }

    /// <inheritdoc/>
    required public ILiteralExpression? Initializer { get; init; }

    /// <inheritdoc/>
    required public MethodName MethodName { get; init; }
}
