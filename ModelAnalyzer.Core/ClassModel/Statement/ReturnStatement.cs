namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("return {Expression}")]
public class ReturnStatement : IStatement
{
    required public IExpression? Expression { get; init; }
}
