namespace ModelAnalyzer;

/// <summary>
/// Represents the context of a parsed arithmetic expression.
/// </summary>
internal class ArithmeticExpressionEntry
{
    /// <summary>
    /// Gets the parsed expression.
    /// </summary>
    required public IArithmeticExpression Expression { get; init; }

    /// <summary>
    /// Gets the host method where the expression is found.
    /// </summary>
    required public Method? HostMethod { get; init; }

    /// <summary>
    /// Gets the call location.
    /// </summary>
    required public ICallLocation CallLocation { get; init; }
}
