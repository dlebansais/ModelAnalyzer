namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class ExpressionTest
{
    [Test]
    [Category("Core")]
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

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void BinaryExpressionTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_1
{
    int X;

    void Write(int x)
    {
        X = x + 1 + x;
        X = x - 1 - x;
        X = x * 1 * x;
        X = x / 1 / x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void BinaryExpressionTest_InvalidOperator()
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

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
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
    [Category("Core")]
    public void BinaryConditionalTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_3
{
    void Write(int x)
    {
        if ((x == 0) && (x == 0 || x == 1) && (x == 0 || x == 1 || x == 2))
            if (true || false)
                if (true && true)
                {
                }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void BinaryConditionalTest_InvalidOperator()
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

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
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

    [Test]
    [Category("Core")]
    public void ParenthesizedExpressionTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_5
{
    int X;

    void Write(int x)
    {
        X = ((x + 1) + (x - 1)) - ((x - 1) - (x + 1));
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ExpressionInAssertionTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_6
{
    int X;

    void Write(int x)
    // Require: x >= 0
    // Require: (x + 1) >= (0)
    // Require: true
    {
        X = x;
    }
    // Ensure: (X) >= 0
    // Ensure: X >= 0 && (X >= 1)
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreExpression_6
  int X
  void Write(x)
    require x >= 0
    require x + 1 >= 0
    require True
    ensure X >= 0
    ensure X >= 0 && X >= 1
"));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedExpressionTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_7
{
    int X;

    void Write(int x)
    {
        if (x is string)
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedExpressionTest_Nested()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_8
{
    int X;

    void Write(int x)
    {
        if (x is string || x is double)
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(5));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedExpressionTest_InvalidFieldName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_9
{
    int X;

    void Write(int x)
    {
        if (Y == 0)
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedExpressionTest_Parenthesized()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_10
{
    int X;

    void Write(int x)
    {
        if ((x is string))
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(3));
    }
}
