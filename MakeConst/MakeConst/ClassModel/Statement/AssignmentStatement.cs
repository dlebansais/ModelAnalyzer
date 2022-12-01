namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("{Destination} = {Expression}")]
public class AssignmentStatement : IStatement
{
    public required IVariable Destination { get; init; }
    public required IExpression Expression { get; init; }
}
