namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class MethodTest
{
    [Test]
    [Category("Core")]
    public void BasicTest_NoReturnValue()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethod_0
{
    int X;

    void Write(int x, int y)
    {
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreMethod_0
  int X
  void Write(x, y)
"));
    }

    [Test]
    [Category("Core")]
    public void BasicTest_WithReturnValue()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethod_1
{
    int X;

    int Read()
    {
        return X;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreMethod_1
  int X
  int Read()
"));
    }

    [Test]
    [Category("Core")]
    public void UnsupportedMethodTest_InvalidReturnType()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethod_2
{
    int X;

    string Write(int x)
    {
        X = x;
        return ""*"";
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Methods.Count, Is.EqualTo(1));

        IUnsupportedMethod UnsupportedMethod = ClassModel.Unsupported.Methods[0];
        Assert.That(UnsupportedMethod.Name, Is.EqualTo("*"));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreMethod_2
  int X
"));
    }
}
