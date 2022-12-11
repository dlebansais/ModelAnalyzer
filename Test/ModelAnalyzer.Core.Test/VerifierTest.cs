namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class VerifierTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreVerifier_0
{
    int X;

    void Write(int x)
    // Require: x >= 0
    {
        X = x;
    }
    // Ensure: X >= 0
}
// Invariant: X >= 0 || X < 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel.IsInvariantViolated, Is.False);
    }

    [Test]
    [Category("Core")]
    public void VerifierTest_ViolationAtDepth0()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreVerifier_1
{
    int X;

    void Write(int x)
    // Require: x >= 0
    {
        X = x;
    }
    // Ensure: X >= 0
}
// Invariant: X < 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel.IsInvariantViolated, Is.True);
    }

    [Test]
    [Category("Core")]
    public void VerifierTest_ViolationAtDepth1()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreVerifier_2
{
    int X;

    void Write(int x)
    // Require: x >= 0
    {
        X = x;
    }
    // Ensure: X >= 0
}
// Invariant: X == 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel.IsInvariantViolated, Is.True);
    }

    [Test]
    [Category("Core")]
    public void VerifierTest_WithUnsupportedField()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreVerifier_3
{
    int X;
    string Y;

    void Write(int x)
    // Require: x >= 0
    {
        X = x;
    }
    // Ensure: X >= 0
}
// Invariant: X == 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.IsInvariantViolated, Is.False);
    }

    [Test]
    [Category("Core")]
    public void VerifierTest_WithUnsupportedMethod()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreVerifier_4
{
    int X;

    void Write(int x)
    // Require: x >= 0
    {
        X = x;
    }
    // Ensure: X >= 0

    string Read()
    {
        return ""*"";
    }
}
// Invariant: X == 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.IsInvariantViolated, Is.False);
    }

    [Test]
    [Category("Core")]
    public void VerifierTest_WithUnsupportedParameter()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreVerifier_5
{
    int X;

    void Write1(int x)
    // Require: x >= 0
    {
        X = x;
    }
    // Ensure: X >= 0

    void Write2(string x)
    {
    }
}
// Invariant: X == 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.IsInvariantViolated, Is.False);
    }

    [Test]
    [Category("Core")]
    public void VerifierTest_WithUnsupportedExpression()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreVerifier_6
{
    int X;

    void Write(int x)
    // Require: x >= 0
    {
        X = typeof(X) is null ? x : 0;
    }
    // Ensure: X >= 0
}
// Invariant: X == 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.IsInvariantViolated, Is.False);
    }
}
