namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class RequireTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_0
{
    int X;

    void Write(int x)
    // Require: x >= 0
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_0
  int X
  void Write(x)
    require x >= 0
"));
    }

    [Test]
    [Category("Core")]
    public void RequireTest_ComplexRequire()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_1
{
    int X;

    void Write(int x)
    // Require: x >= 0 || x >= (0 + 1) || (x + 1) <= 0
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_1
  int X
  void Write(x)
    require (x >= 0) || (x >= 0 + 1) || (x + 1) <= 0
"));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedRequireTest_ExpressionNotBoolean()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_2
{
    int X;

    void Write(int x)
    // Require: 0
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Requires.Count, Is.EqualTo(1));

        IUnsupportedRequire UnsupportedRequire = ClassModel.Unsupported.Requires[0];
        Assert.That(UnsupportedRequire.Text, Is.EqualTo("0"));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_2
  int X
  void Write(x)
"));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedRequireTest_TooManyInstructions()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_3
{
    int X;

    void Write(int x)
    // Require: X == 0; break;
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Requires.Count, Is.EqualTo(1));

        IUnsupportedRequire UnsupportedRequire = ClassModel.Unsupported.Requires[0];
        Assert.That(UnsupportedRequire.Text, Is.EqualTo("X == 0; break;"));
    }

    [Test]
    [Category("Core")]
    public void UnparsedRequireTest_NoKeyword()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_4
{
    int X;

    void Write(int x)
    // Require
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_4
  int X
  void Write(x)
"));
    }

    [Test]
    [Category("Core")]
    public void RequireTest_MultipleMethods()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_5
{
    int X;

    void Write1(int x)
    // Require: x >= 0
    {
        X = x;
    }

    void Write2(int x)
    // Require: x >= 0
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_5
  int X
  void Write1(x)
    require x >= 0
  void Write2(x)
    require x >= 0
"));
    }

    [Test]
    [Category("Core")]
    public void RequireTest_UnsupportedMember()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_6
{
    int X;

    void Write(int x)
    // Require: x >= 0
    {
        X = x;
    }

    int Getter { get; }
    // Require: X >= 0
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.HasUnsupporteMember, Is.True);
        Assert.That(ClassModel.Unsupported.Requires.Count, Is.EqualTo(1));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_6
  int X
  void Write(x)
    require x >= 0
"));
    }

    [Test]
    [Category("Core")]
    public void RequireTest_ExpressionBody()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_7
{
    int X;

    int Read() => X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_7
  int X
  int Read()
"));
    }
}
