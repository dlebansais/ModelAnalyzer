namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents an assignment statement.
/// </summary>
[DebuggerDisplay("{Destination} = {Expression}")]
internal class AssignmentStatement : Statement
{
    /// <summary>
    /// Gets the destination variable.
    /// </summary>
    required public IVariable Destination { get; init; }

    /// <summary>
    /// Gets the source expression.
    /// </summary>
    required public IExpression Expression { get; init; }
}
