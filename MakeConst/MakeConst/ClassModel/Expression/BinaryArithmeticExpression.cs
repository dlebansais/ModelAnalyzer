namespace DemoAnalyzer;

using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;

[DebuggerDisplay("{Left} {OperatorText} {Right}")]
public class BinaryArithmeticExpression : IExpression
{
    public required IExpression Left { get; init; }
    public required SyntaxKind OperatorKind { get; init; }
    public required IExpression Right { get; init; }
}
