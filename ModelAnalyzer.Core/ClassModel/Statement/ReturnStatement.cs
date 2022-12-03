namespace DemoAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a return statement.
/// </summary>
[DebuggerDisplay("return {Expression}")]
internal class ReturnStatement : Statement
{
    /// <summary>
    /// Gets the expression giving the returned value.
    /// </summary>
    required public IExpression? Expression { get; init; }
}
