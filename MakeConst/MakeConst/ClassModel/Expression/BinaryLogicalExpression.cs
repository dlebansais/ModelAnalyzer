namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("{Left} {OperatorText} {Right}")]
public class BinaryLogicalExpression : IExpression
{
    public required IExpression Left { get; init; }
    public required string OperatorText { get; init; }
    public required IExpression Right { get; init; }
}
