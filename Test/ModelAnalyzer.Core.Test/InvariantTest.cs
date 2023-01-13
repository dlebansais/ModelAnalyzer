namespace Core.Test;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Miscellaneous.Test;
using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Invariant"/> class.
/// </summary>
public class InvariantTest
{
    [Test]
    [Category("Core")]
    public void Invariant_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_0
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}
// Invariant: X >= 0 || X < 0
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        IList<IInvariant> Invariants = ClassModel.GetInvariants();

        Assert.That(Invariants.Count, Is.EqualTo(1));

        IInvariant FirstInvariant = Invariants.First();

        Assert.That(FirstInvariant.Text, Is.EqualTo("X >= 0 || X < 0"));
        Assert.That(FirstInvariant.Location.IsInSource, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreInvariant_0
  int X

  public void Write(int x)
  {
    X = x;
  }
  * (X >= 0) || (X < 0)
"));
    }

    [Test]
    [Category("Core")]
    public void Invariant_ExpressionNotBoolean()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_1
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}
// Invariant: 0
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Invariants.Count, Is.EqualTo(1));

        IUnsupportedInvariant UnsupportedInvariant = ClassModel.Unsupported.Invariants[0];
        Assert.That(UnsupportedInvariant.Text, Is.EqualTo("0"));
    }

    [Test]
    [Category("Core")]
    public void Invariant_InvalidExpression()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_2
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}
// Invariant: *
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Invariants.Count, Is.EqualTo(1));

        IUnsupportedInvariant UnsupportedInvariant = ClassModel.Unsupported.Invariants[0];
        Assert.That(UnsupportedInvariant.Text, Is.EqualTo("*"));
    }

    [Test]
    [Category("Core")]
    public void Invariant_TooManyInstructions()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_3
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}
// Invariant: X == 0; break;
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Invariants.Count, Is.EqualTo(1));

        IUnsupportedInvariant UnsupportedInvariant = ClassModel.Unsupported.Invariants[0];
        Assert.That(UnsupportedInvariant.Text, Is.EqualTo("X == 0; break;"));
    }

    [Test]
    [Category("Core")]
    public void Invariant_NoNewLineAtEndofFile()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_4
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Invariant_InNamespace()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
namespace Invariant
{
    using System;

    class Program_CoreInvariant_5
    {
        int X;

        void Write(int x)
        {
            X = x;
        }
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Invariant_NoKeyword()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_6
{
    int X;

    void Write(int x)
    {
        X = x;
    }
}
// Invariant
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Invariant_UnknownField()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreInvariant_7
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}
// Invariant: Y == 0
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }
}
