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
    public void BinaryArithmeticExpressionTest()
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
    public void BinaryArithmeticExpressionTest_InvalidOperator()
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

    [Test]
    [Category("Core")]
    public void BinaryLogicalTest()
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
    public void BinaryLogicalTest_InvalidOperator()
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

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateBinaryLogicalOperator, SyntaxKind.DoKeyword);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void UnaryLogicalTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_5
{
    void Write(int x)
    {
        if (!(x == 0))
            if (!true)
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
    public void UnaryLogicalTest_InvalidOperator()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_6
{
    int X;

    void Write(int x)
    {
        if (!(x == 0)) // Token '!' is replaced with '%'.
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateUnaryLogicalOperator, SyntaxKind.PercentToken);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void ParenthesizedExpressionTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_7
{
    int X;

    void Write(int x)
    {
        X = ((x + 1) + (x - 1)) - ((x - 1) - (x + 1) + (-(x + 1)));
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

class Program_CoreExpression_8
{
    int X;

    void Write(int x)
    // Require: x >= 0
    // Require: (x + 1) >= (0)
    // Require: true
    // Require: (x + 1) >= -1
    {
        X = x;
    }
    // Ensure: (X) >= 0
    // Ensure: X >= 0 && (X >= 1)
    // Ensure: !(X < 0)
    // Ensure: X >= 0 && !(X >= 1)
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreExpression_8
  int X
  void Write(int x)
  # require x >= 0
  # require (x + 1) >= 0
  # require true
  # require (x + 1) >= -1
  {
    X = x;
  }
  # ensure X >= 0
  # ensure (X >= 0) && (X >= 1)
  # ensure !(X < 0)
  # ensure (X >= 0) && (!(X >= 1))
"));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedExpressionTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_9
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
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedExpressionTest_Nested()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_10
{
    int X;

    void Write(int x)
    {
        if (x is string || x is float)
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
    public void UnsupportedExpressionTest_InvalidFieldName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_11
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
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedExpressionTest_Parenthesized()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_12
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
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedExpressionTest_Literal()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_13
{
    int X;

    void Write(int x)
    {
        if (x == ""*"")
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void UnaryArithmeticExpressionTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_14
{
    int X;

    void Write()
    {
        X = -1;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void UnaryArithmeticExpressionTest_InvalidOperator()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_15
{
    int X;

    void Write(int x)
    {
        X = -1; // Token '-' is replaced with '+'.
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateUnaryArithmeticOperator, SyntaxKind.PlusToken);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void UnaryArithmeticExpressionTest_InvalidOperand()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_16
{
    int X;

    void Write(int x)
    {
        X = -sizeof(X);
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void ExpressionTest_SourceAndDestinationNotCompatible()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_17
{
    int X;

    void Write(int x, double y)
    {
        X = x + y;
        X = y + x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Statements.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void ExpressionTest_Double()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_18
{
    double X;

    void Write()
    {
        X = 1.1 + 2.2;
        X = 1;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
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

    private SyntaxToken LocateUnaryArithmeticOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)Block.Statements[0];
        AssignmentExpressionSyntax AssignmentExpression = (AssignmentExpressionSyntax)ExpressionStatement.Expression;
        PrefixUnaryExpressionSyntax UnaryExpression = (PrefixUnaryExpressionSyntax)AssignmentExpression.Right;
        SyntaxToken Operator = UnaryExpression.OperatorToken;

        return Operator;
    }

    private SyntaxToken LocateBinaryLogicalOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        IfStatementSyntax IfStatement = (IfStatementSyntax)Block.Statements[0];
        BinaryExpressionSyntax BinaryExpression = (BinaryExpressionSyntax)IfStatement.Condition;
        SyntaxToken Operator = BinaryExpression.OperatorToken;

        return Operator;
    }

    private SyntaxToken LocateUnaryLogicalOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        IfStatementSyntax IfStatement = (IfStatementSyntax)Block.Statements[0];
        PrefixUnaryExpressionSyntax UnaryExpression = (PrefixUnaryExpressionSyntax)IfStatement.Condition;
        SyntaxToken Operator = UnaryExpression.OperatorToken;

        return Operator;
    }
}
