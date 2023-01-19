namespace ModelAnalyzer;

/// <summary>
/// Represents the context of a parsed function call expression.
/// </summary>
internal class FunctionCallStatementEntry
{
    /// <summary>
    /// Gets the parsed statement.
    /// </summary>
    required public IFunctionCallExpression Expression { get; init; }

    /// <summary>
    /// Gets the host method where the statement is found.
    /// </summary>
    required public Method? HostMethod { get; init; }

    /// <summary>
    /// Gets the call location.
    /// </summary>
    required public ICallLocation CallLocation { get; init; }
}
