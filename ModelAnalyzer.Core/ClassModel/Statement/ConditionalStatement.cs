namespace DemoAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

[DebuggerDisplay("if ({Condition}) then ...")]
public class ConditionalStatement : IStatement
{
    required public IExpression Condition { get; init; }
    required public List<IStatement> WhenTrueStatementList { get; init; }
    required public List<IStatement> WhenFalseStatementList { get; init; }
}
