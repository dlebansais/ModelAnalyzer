namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class InvariantTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_0
{
    int X;

    void Write(int x)
    {
        X = x;
    }
}
// Invariant: X >= 0 || X < 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreInvariant_0
  int X
  void Write(x)
  {
    X = x;
  }
  * (X >= 0) || (X < 0)
"));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedInvariantTest_ExpressionNotBoolean()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_1
{
    int X;

    void Write(int x)
    {
        X = x;
    }
}
// Invariant: 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Invariants.Count, Is.EqualTo(1));

        IUnsupportedInvariant UnsupportedInvariant = ClassModel.Unsupported.Invariants[0];
        Assert.That(UnsupportedInvariant.Text, Is.EqualTo("0"));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedInvariantTest_TooManyInstructions()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_2
{
    int X;

    void Write(int x)
    {
        X = x;
    }
}
// Invariant: X == 0; break;
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Invariants.Count, Is.EqualTo(1));

        IUnsupportedInvariant UnsupportedInvariant = ClassModel.Unsupported.Invariants[0];
        Assert.That(UnsupportedInvariant.Text, Is.EqualTo("X == 0; break;"));
    }

    [Test]
    [Category("Core")]
    public void InvariantTest_NoNewLineAtEndofFile()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_3
{
    int X;

    void Write(int x)
    {
        X = x;
    }
}");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void InvariantTest_InNamespace()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
namespace Invariant
{
    using System;

    class Program_CoreInvariant_4
    {
        int X;

        void Write(int x)
        {
            X = x;
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
    public void InvariantTest_NoKeyword()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_5
{
    int X;

    void Write(int x)
    {
        X = x;
    }
}
// Invariant
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }
}
