namespace DemoAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

[DebuggerDisplay("if ({Condition}) then ...")]
public class ConditionalStatement : IStatement
{
    public required IExpression Condition { get; init; }
    public required List<IStatement> WhenTrueStatementList { get; init; }
    public required List<IStatement> WhenFalseStatementList { get; init; }
}
