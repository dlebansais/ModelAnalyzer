namespace ModelAnalyzer.Core.Test;

using System.Threading.Tasks;
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
    }

    [Test]
    [Category("Core")]
    public void UnsupportedEnsureTest_ExpressionNotBoolean()
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

class Program_CoreEnsure_0
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
}
