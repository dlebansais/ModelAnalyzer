namespace ModelAnalyzer.Core.Test;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using ProcessCommunication;

public class ClassModelManagerTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_0
{
}
");

        bool IsIgnored = ClassModelManager.IsClassIgnoredForModeling(ClassDeclaration);

        Assert.IsFalse(IsIgnored);

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClassModelManager_0
"));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_NoModelTrivia()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

// No model
class Program_CoreClassModelManager_1
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
class Program_CoreClassModelManager_2
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

class Program_CoreClassModelManager_3
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: false, manager => RemoveClasses(manager, new List<string>() { "Program_CoreClassModelManager_3" }));

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClassModelManager_3
"));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_RemoveAllClasses()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_4
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: false, manager => RemoveClasses(manager, new List<string>()));

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClassModelManager_4
"));
    }

    [Test]
    [Category("Core")]
    public async Task ClassModelManagerTest_DuplicateVerification()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_5
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        List<IClassModel> ClassModelList = await TestHelper.ToClassModelAsync(new List<ClassDeclarationSyntax>() { ClassDeclaration, ClassDeclaration }, TokenReplacement, manager => RemoveClasses(manager, new List<string>() { "Program_CoreClassModelManager_5" }));
        Assert.That(ClassModelList.Count, Is.EqualTo(2));
        ClassModel ClassModel1 = (ClassModel)ClassModelList[0];
        ClassModel ClassModel2 = (ClassModel)ClassModelList[1];

        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel2.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_EmptyClassName()
    {
        ClassDeclarationSyntax ClassDeclaration = SyntaxFactory.ClassDeclaration(string.Empty);

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        Assert.Throws<ArgumentException>(() => TestHelper.ToClassModel(ClassDeclaration, TokenReplacement));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_EmptyClassNameAsync()
    {
        ClassDeclarationSyntax ClassDeclaration = SyntaxFactory.ClassDeclaration(string.Empty);

        bool IsIgnored = ClassModelManager.IsClassIgnoredForModeling(ClassDeclaration);

        Assert.IsTrue(IsIgnored);

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        Assert.ThrowsAsync<ArgumentException>(() => TestHelper.ToClassModelAsync(ClassDeclaration, TokenReplacement));
    }

    [Test]
    [Category("Core")]
    public async Task ClassModelManagerTest_DuplicateClassRemoved()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_6
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        List<IClassModel> ClassModelList = await TestHelper.ToClassModelAsync(new List<ClassDeclarationSyntax>() { ClassDeclaration, ClassDeclaration }, TokenReplacement, manager => RemoveClasses(manager, new List<string>()));
        Assert.That(ClassModelList.Count, Is.EqualTo(2));
        ClassModel ClassModel1 = (ClassModel)ClassModelList[0];
        ClassModel ClassModel2 = (ClassModel)ClassModelList[1];

        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel2.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_DuplicateVerificationWithUpdate()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_7
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, DuplicateVerificationWithUpdate);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_VerificationWithErrorAndUpdate()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_8
{
    string X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, VerificationWithErrorAndUpdate);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_UpdateWithAutoStart()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_9
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, UpdateWithAutoStart);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_UpdateWithBlockedChannel()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_10
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, UpdateWithBlockedChannel);
    }

    private void RemoveClasses(ClassModelManager manager, List<string> existingClassNameList)
    {
        manager.RemoveMissingClasses(existingClassNameList);
    }

    private void DuplicateVerificationWithUpdate(List<ClassDeclarationSyntax> classDeclarationList)
    {
        ClassDeclarationSyntax ClassDeclaration = classDeclarationList[0];
        IClassModel ClassModel;

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModel(CompilationContext.GetAnother(), ClassDeclaration);
        ClassModel = Manager.GetClassModel(CompilationContext.GetAnother(), ClassDeclaration);

        Manager.GetVerifiedModel(ClassModel);
    }

    private void VerificationWithErrorAndUpdate(List<ClassDeclarationSyntax> classDeclarationList)
    {
        ClassDeclarationSyntax ClassDeclaration = classDeclarationList[0];
        IClassModel ClassModel;

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModel(CompilationContext.GetAnother(), ClassDeclaration);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);

        Manager.GetVerifiedModel(ClassModel);
    }

    private void UpdateWithAutoStart(List<ClassDeclarationSyntax> classDeclarationList)
    {
        ClassDeclarationSyntax ClassDeclaration = classDeclarationList[0];
        IClassModel ClassModel;

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Auto };

        ClassModel = Manager.GetClassModel(CompilationContext.GetAnother(), ClassDeclaration);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        Manager.GetVerifiedModel(ClassModel);
    }

    private void UpdateWithBlockedChannel(List<ClassDeclarationSyntax> classDeclarationList)
    {
        ClassDeclarationSyntax ClassDeclaration = classDeclarationList[0];
        IClassModel ClassModel;

        Guid ServerToClientGuid = new Guid("{30AC0040-5412-4DF0-AAB3-28D66599E7D2}");

        using Channel Channel = new Channel(ServerToClientGuid, Mode.Receive);
        Channel.Open();

        using ClassModelManager Manager = new(ServerToClientGuid) { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModel(CompilationContext.GetAnother(), ClassDeclaration);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        Manager.GetVerifiedModel(ClassModel);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_Dispose()
    {
        using (ClassModelManagerExtended TestObject = new ClassModelManagerExtended())
        {
        }
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_DoubleDispose()
    {
        using (ClassModelManagerExtended TestObject = new ClassModelManagerExtended())
        {
            TestObject.Dispose();
        }
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_FakeFinalize()
    {
        using (ClassModelManagerExtended TestObject = new ClassModelManagerExtended())
        {
            TestObject.FakeFinalize();
        }
    }

    [Test]
    [Category("Core")]
    public void ClassModelManagerTest_Destructor()
    {
        using ClassModelManagerContainer Container = new();
    }
}
