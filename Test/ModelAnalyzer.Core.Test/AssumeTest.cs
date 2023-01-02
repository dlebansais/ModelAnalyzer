namespace ModelAnalyzer.Core.Test;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

/// <summary>
/// Tests for flow check.
/// </summary>
public class AssumeTest
{
    [Test]
    [Category("Core")]
    public void Assume_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreAssume_0
{
    int X;

    public void Write(int x, int y)
    // Require: y > 0
    {
        X = x / y;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel.AssumeViolations.Count, Is.EqualTo(0));

        IList<IMethod> Methods = ClassModel.GetMethods();

        Assert.That(Methods.Count, Is.EqualTo(1));

        IMethod FirstMethod = Methods.First();
        Assert.That(FirstMethod.Name.Text, Is.EqualTo("Write"));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreAssume_0
  int X

  public void Write(int x, int y)
  # require y > 0
  {
    X = x / y;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Assume_WithError()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreAssume_1
{
    int X;

    public void Write(int x, int y)
    {
        X = x / y;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel.AssumeViolations.Count, Is.EqualTo(1));

        IAssumeViolation FirstViolation = ClassModel.AssumeViolations[0];
        Assert.That(FirstViolation.Method, Is.Not.Null);

        IMethod ViolationMethod = FirstViolation.Method!;
        Assert.That(ViolationMethod.Name.Text, Is.EqualTo("Write"));
        Assert.That(FirstViolation.Text, Is.EqualTo("x / y"));
    }

    [Test]
    [Category("Core")]
    public void Assume_WithErrorInInvariant()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreAssume_2
{
    int X;
    int Y;

    public void Write(int x, int y)
    // Require: x >= 0
    // Require: y >= 0
    {
        X = x;
        Y = y;
    }
}
// Invariant: X / Y >= 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel.AssumeViolations.Count, Is.EqualTo(1));

        IAssumeViolation FirstViolation = ClassModel.AssumeViolations[0];
        Assert.That(FirstViolation.Method, Is.Null);
        Assert.That(FirstViolation.Text, Is.EqualTo("X / Y"));
    }
}
