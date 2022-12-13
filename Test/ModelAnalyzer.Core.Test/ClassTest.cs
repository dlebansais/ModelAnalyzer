namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class ClassTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_0
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClass_0
"));
    }

    [Test]
    [Category("Core")]
    public void InvalidClassTest_Attribute()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

[Serializable]
class Program_CoreClass_1
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }

    [Test]
    [Category("Core")]
    public void ClassTest_PublicModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

public class Program_CoreClass_2
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClass_2
"));
    }

    [Test]
    [Category("Core")]
    public void ClassTest_PrivateModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

private class Program_CoreClass_3
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClass_3
"));
    }

    [Test]
    [Category("Core")]
    public void ClassTest_InternalModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

internal class Program_CoreClass_4
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClass_4
"));
    }

    [Test]
    [Category("Core")]
    public void ClassTest_PartialModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

partial class Program_CoreClass_5
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClass_5
"));
    }

    [Test]
    [Category("Core")]
    public void InvalidClassTest_Modifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

static class Program_CoreClass_6
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }

    [Test]
    [Category("Core")]
    public void InvalidClassTest_Base()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_7 : IDisposable
{
    public void Dispose()
    {
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }

    [Test]
    [Category("Core")]
    public void InvalidClassTest_TypeParameter()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_8<T>
{
    T X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }

    [Test]
    [Category("Core")]
    public void InvalidClassTest_Constraint()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_9<T>
    where T : class
{
    T X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }
}
