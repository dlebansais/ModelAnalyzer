namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class EnsureTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_0
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Ensure: X == x
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_0
  int X
  void Write(x)
    ensure X == x
"));
    }

    [Test]
    [Category("Core")]
    public void BasicTest_ComplexEnsure()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_1
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Ensure: X == x || X != (x + 1) || (X + 1) != x
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void UnsupportedEnsureTest_ExpressionNotBoolean()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_2
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Ensure: 0
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Ensures.Count, Is.EqualTo(1));

        IUnsupportedEnsure UnsupportedEnsure = ClassModel.Unsupported.Ensures[0];
        Assert.That(UnsupportedEnsure.Text, Is.EqualTo("0"));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedEnsureTest_TooManyInstructions()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_3
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Ensure: X == 0; break;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Ensures.Count, Is.EqualTo(1));

        IUnsupportedEnsure UnsupportedEnsure = ClassModel.Unsupported.Ensures[0];
        Assert.That(UnsupportedEnsure.Text, Is.EqualTo("X == 0; break;"));
    }

    [Test]
    [Category("Core")]
    public void UnparsedEnsureTest_NoKeyword()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_4
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Ensure
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_4
  int X
  void Write(x)
"));
    }

    [Test]
    [Category("Core")]
    public void EnsureTest_MultipleMethods()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_5
{
    int X;

    void Write1(int x)
    {
        X = x;
    }
    // Ensure: X == x

    void Write2(int x)
    {
        X = x;
    }
    // Ensure: X == x
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_5
  int X
  void Write1(x)
    ensure X == x
  void Write2(x)
    ensure X == x
"));
    }

    [Test]
    [Category("Core")]
    public void EnsureTest_UnsupportedMember()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_6
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Ensure: X == x

    int Getter { get; }
    // Ensure: X == x
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.HasUnsupporteMember, Is.True);
        Assert.That(ClassModel.Unsupported.Ensures.Count, Is.EqualTo(1));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_6
  int X
  void Write(x)
    ensure X == x
"));
    }
}
