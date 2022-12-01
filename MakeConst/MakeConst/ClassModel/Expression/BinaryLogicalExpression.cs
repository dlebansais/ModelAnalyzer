﻿namespace DemoAnalyzer;

using Microsoft.CodeAnalysis.CSharp;

public class BinaryLogicalExpression : IExpression
{
    public bool IsSimple => false;
    public required IExpression Left { get; init; }
    public required SyntaxKind OperatorKind { get; init; }
    public required IExpression Right { get; init; }

    public override string ToString()
    {
        string LeftString = Left.IsSimple ? $"{Left}" : $"({Left})";
        string RightString = Right.IsSimple ? $"{Right}" : $"({Right})";
        return $"{LeftString} {ClassModel.SupportedLogicalOperators[OperatorKind].Text} {RightString}";
    }
}
