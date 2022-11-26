namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("{Value}")]
public class LiteralValueExpression : IExpression
{
    public required int Value { get; init; }
}
