namespace ModelAnalyzer.Core.Test;

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Z3;
using NUnit.Framework;

public class ClassManagerTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassManager_0
{
}
");

        bool IsIgnored = ClassModelManager.IsClassIgnoredForModeling(ClassDeclaration);

        Assert.IsFalse(IsIgnored);

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClassManager_0
"));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_NoModelTrivia()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

// No model
class Program_CoreClassManager_1
{
}
");

        bool IsIgnored = ClassModelManager.IsClassIgnoredForModeling(ClassDeclaration);

        Assert.IsTrue(IsIgnored);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_IgnoredTrivia()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

// None
class Program_CoreClassManager_2
{
}
");

        bool IsIgnored = ClassModelManager.IsClassIgnoredForModeling(ClassDeclaration);

        Assert.IsFalse(IsIgnored);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_RemoveNoClass()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassManager_3
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: false, manager => RemoveClasses(manager, new List<string>() { "Program_CoreClassManager_3" }));

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClassManager_3
"));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_RemoveAllClasses()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassManager_4
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: false, manager => RemoveClasses(manager, new List<string>()));

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClassManager_4
"));
    }

    private void RemoveClasses(ClassModelManager manager, List<string> existingClassNameList)
    {
        manager.RemoveMissingClasses(existingClassNameList);
    }
}
