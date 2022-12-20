namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class ParameterTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_0
{
    int X;
    bool Y;

    void Write(int x, bool y)
    {
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void UnsupportedParameterTest_InvalidParameterType()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_2
{
    void Write(string x)
    {
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Methods.Count, Is.EqualTo(0));
        Assert.That(ClassModel.Unsupported.Parameters.Count, Is.EqualTo(1));

        IUnsupportedParameter UnsupportedParameter = ClassModel.Unsupported.Parameters[0];
        Assert.That(UnsupportedParameter.Name, Is.EqualTo("*"));
    }

    [Test]
    [Category("Core")]
    public void ParameterTest_DuplicateParameterName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_3
{
    void Write(int x, int x)
    {
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreParameter_3
  void Write(int x)
  {
  }
"));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedParameterTest_Attribute()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_4
{
    void Write([System.Runtime.InteropServices.In]int x)
    {
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Methods.Count, Is.EqualTo(0));
        Assert.That(ClassModel.Unsupported.Parameters.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedParameterTest_Modifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_5
{
    void Write(ref int x)
    {
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Methods.Count, Is.EqualTo(0));
        Assert.That(ClassModel.Unsupported.Parameters.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedParameterTest_NameCollisionWithField()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_6
{
    int X;

    void Write(int X)
    {
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Methods.Count, Is.EqualTo(0));
        Assert.That(ClassModel.Unsupported.Parameters.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void ParameterTest_InvalidParameterTestedAsSource()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_7
{
    int Y;

    void Write(string x, int y)
    {
        Y = y;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }
}
