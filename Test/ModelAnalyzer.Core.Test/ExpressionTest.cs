namespace ModelAnalyzer.Core.Test;

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

[TestFixture]
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

        TokenManipulationMutex.WaitOne();

        ClassModelManager Manager = new();
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext.Default, ClassDeclaration);

        TokenManipulationMutex.ReleaseMutex();

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

        TokenManipulationMutex.WaitOne();

        ClassModelManager Manager = new();
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext.Default, ClassDeclaration);

        TokenManipulationMutex.ReleaseMutex();

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

        TokenManipulationMutex.WaitOne();

        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)ClassDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)Block.Statements[0];
        AssignmentExpressionSyntax AssignmentExpression = (AssignmentExpressionSyntax)ExpressionStatement.Expression;
        BinaryExpressionSyntax BinaryExpression = (BinaryExpressionSyntax)AssignmentExpression.Right;
        SyntaxToken Operator = BinaryExpression.OperatorToken;

        TestHelper.ReplaceTokenKind(Operator, SyntaxKind.SemicolonToken, out SyntaxKind OldKind);

        ClassModelManager Manager = new();
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext.Default, ClassDeclaration);

        TestHelper.ReplaceTokenKind(Operator, OldKind, out _);

        TokenManipulationMutex.ReleaseMutex();

        Assert.IsTrue(OldKind == SyntaxKind.PlusToken);
        Assert.IsFalse(ClassModel.Unsupported.IsEmpty);
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

        TokenManipulationMutex.WaitOne();

        ClassModelManager Manager = new();
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext.Default, ClassDeclaration);

        TokenManipulationMutex.ReleaseMutex();

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

        TokenManipulationMutex.WaitOne();

        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)ClassDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        IfStatementSyntax IfStatement = (IfStatementSyntax)Block.Statements[0];
        BinaryExpressionSyntax BinaryExpression = (BinaryExpressionSyntax)IfStatement.Condition;
        SyntaxToken Operator = BinaryExpression.OperatorToken;

        TestHelper.ReplaceTokenKind(Operator, SyntaxKind.DoKeyword, out SyntaxKind OldKind);

        ClassModelManager Manager = new();
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext.Default, ClassDeclaration);

        TestHelper.ReplaceTokenKind(Operator, OldKind, out _);

        TokenManipulationMutex.ReleaseMutex();

        Assert.IsTrue(OldKind == SyntaxKind.AmpersandAmpersandToken);
        Assert.IsFalse(ClassModel.Unsupported.IsEmpty);
    }

    private static Mutex TokenManipulationMutex = new();
}
