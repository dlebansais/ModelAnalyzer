namespace DemoAnalyzer;

using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

[DebuggerDisplay("{Left} {OperatorText} {Right}")]
public class ComparisonExpression : IExpression
{
    public required IExpression Left { get; init; }
    public required SyntaxKind OperatorKind { get; init; }
    public required IExpression Right { get; init; }
}
