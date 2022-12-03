namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("return {Expression}")]
internal class ReturnStatement : IStatement
{
    required public IExpression? Expression { get; init; }
}
