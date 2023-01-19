namespace ModelAnalyzer;

/// <summary>
/// Represents the context of a parsed method call statement.
/// </summary>
internal class MethodCallStatementEntry
{
    /// <summary>
    /// Gets the parsed statement.
    /// </summary>
    required public IMethodCallStatement Statement { get; init; }

    /// <summary>
    /// Gets the host method where the statement is found.
    /// </summary>
    required public Method HostMethod { get; init; }

    /// <summary>
    /// Gets the call location.
    /// </summary>
    required public ICallLocation CallLocation { get; init; }
}
