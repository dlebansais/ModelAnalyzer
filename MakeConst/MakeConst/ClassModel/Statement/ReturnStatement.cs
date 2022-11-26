namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("return {Expression}")]
public class ReturnStatement : IStatement
{
    public required IExpression? Expression { get; init; }
}
