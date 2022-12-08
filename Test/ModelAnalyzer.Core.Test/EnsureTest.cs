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

        ClassModelManager Manager = new();
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext.Default, ClassDeclaration);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }
}
