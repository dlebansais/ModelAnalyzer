namespace ModelAnalyzer;

/// <summary>
/// Represents the context of a parsed function call expression.
/// </summary>
internal class FunctionCallExpressionEntry
{
    /// <summary>
    /// Gets the parsed expression.
    /// </summary>
    required public IFunctionCallExpression Expression { get; init; }

    /// <summary>
    /// Gets the host method where the expression is found.
    /// </summary>
    required public Method? HostMethod { get; init; }

    /// <summary>
    /// Gets the call location.
    /// </summary>
    required public ICallLocation CallLocation { get; init; }
}
