namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class FieldTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_0
{
    int X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void UnsupportedFieldTest_BadType()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_1
{
    string X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Fields.Count, Is.EqualTo(1));

        IUnsupportedField UnsupportedField = ClassModel.Unsupported.Fields[0];
        Assert.That(UnsupportedField.Name, Is.EqualTo("*"));
    }
}
