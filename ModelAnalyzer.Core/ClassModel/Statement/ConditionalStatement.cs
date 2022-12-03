namespace DemoAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a conditional statement.
/// </summary>
[DebuggerDisplay("if ({Condition}) then ...")]
internal class ConditionalStatement : Statement
{
    /// <summary>
    /// Gets the condition.
    /// </summary>
    required public IExpression Condition { get; init; }

    /// <summary>
    /// Gets statements when the condition is true.
    /// </summary>
    required public List<IStatement> WhenTrueStatementList { get; init; }

    /// <summary>
    /// Gets statements when the condition is false.
    /// </summary>
    required public List<IStatement> WhenFalseStatementList { get; init; }
}
