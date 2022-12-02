namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("{Destination} = {Expression}")]
public class AssignmentStatement : IStatement
{
    required public IVariable Destination { get; init; }
    required public IExpression Expression { get; init; }
}
