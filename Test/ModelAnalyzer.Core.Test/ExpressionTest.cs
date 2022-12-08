namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class ExpressionTest
{
    [Test]
    public void BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_0
{
    int X;

    void Write(int x)
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.IsTrue(ClassModel.Unsupported.IsEmpty);
    }

    [Test]
    public void BinaryExpressionTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_1
{
    int X;

    void Write(int x)
    {
        X = x + 1;
        X = x - 1;
        X = x * 1;
        X = x / 1;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.IsTrue(ClassModel.Unsupported.IsEmpty);
    }

    [Test]
    public void BinaryExpressionInvalidOperatorTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_2
{
    int X;

    void Write(int x)
    {
        X = x + 1; // Token '+' is replaced with ';'.
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateBinaryArithmeticOperator, SyntaxKind.SemicolonToken);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.IsFalse(ClassModel.Unsupported.IsEmpty);
    }

    private SyntaxToken LocateBinaryArithmeticOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)Block.Statements[0];
        AssignmentExpressionSyntax AssignmentExpression = (AssignmentExpressionSyntax)ExpressionStatement.Expression;
        BinaryExpressionSyntax BinaryExpression = (BinaryExpressionSyntax)AssignmentExpression.Right;
        SyntaxToken Operator = BinaryExpression.OperatorToken;

        return Operator;
    }

    [Test]
    public void BinaryConditionalTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_3
{
    void Write(int x)
    {
        if (x == 0 && x == 0)
            if (x == 0 || x == 0)
            {
            }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.IsTrue(ClassModel.Unsupported.IsEmpty);
    }

    [Test]
    public void BinaryConditionalInvalidOperatorTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_4
{
    int X;

    void Write(int x)
    {
        if (x == 0 && x == 0) // Token '&&' is replaced with 'do'.
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateBinaryConditionalOperator, SyntaxKind.DoKeyword);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.IsFalse(ClassModel.Unsupported.IsEmpty);
    }

    private SyntaxToken LocateBinaryConditionalOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        IfStatementSyntax IfStatement = (IfStatementSyntax)Block.Statements[0];
        BinaryExpressionSyntax BinaryExpression = (BinaryExpressionSyntax)IfStatement.Condition;
        SyntaxToken Operator = BinaryExpression.OperatorToken;

        return Operator;
    }
}
