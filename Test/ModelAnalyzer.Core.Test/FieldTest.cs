namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class FieldTest
{
    [Test]
    public void BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_0
{
    int X;
}
");

        ClassModelManager Manager = new();
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext.Default, ClassDeclaration);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    public void UnsupportedFieldTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_0
{
    string X;
}
");

        ClassModelManager Manager = new();
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext.Default, ClassDeclaration);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Fields.Count, Is.EqualTo(1));

        IUnsupportedField UnsupportedField = ClassModel.Unsupported.Fields[0];
        Assert.That(UnsupportedField.Name, Is.EqualTo("*"));
    }
}
