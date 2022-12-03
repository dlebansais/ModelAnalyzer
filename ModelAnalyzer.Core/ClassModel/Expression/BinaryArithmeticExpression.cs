namespace DemoAnalyzer;

using Microsoft.CodeAnalysis.CSharp;

internal class BinaryArithmeticExpression : IExpression
{
    public bool IsSimple => false;
    required public IExpression Left { get; init; }
    required public SyntaxKind OperatorKind { get; init; }
    required public IExpression Right { get; init; }

    public override string ToString()
    {
        string LeftString = Left.IsSimple ? $"{Left}" : $"({Left})";
        string RightString = Right.IsSimple ? $"{Right}" : $"({Right})";
        return $"{LeftString} {SupportedOperators.Arithmetic[OperatorKind].Text} {RightString}";
    }
}
