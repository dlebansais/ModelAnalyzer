namespace ClassModelManager.Test;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FileExtractor;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Miscellaneous.Test;
using ModelAnalyzer;
using NUnit.Framework;
using ProcessCommunication;

/// <summary>
/// Tests for the <see cref="ClassModelManager"/> class.
/// </summary>
public partial class ClassModelManagerTest
{
    [Test]
    [Category("Core")]
    public void ClassModelManager_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_0
{
}
").First();

        bool IsIgnored = ClassModelManager.IsClassIgnoredForModeling(ClassDeclaration);

        Assert.That(IsIgnored, Is.False);

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClassModelManager_0
"));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_NoModelTrivia()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

// No model
class Program_CoreClassModelManager_1
{
}
").First();

        bool IsIgnored = ClassModelManager.IsClassIgnoredForModeling(ClassDeclaration);

        Assert.That(IsIgnored, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_IgnoredTrivia()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

// None
class Program_CoreClassModelManager_2
{
}
").First();

        bool IsIgnored = ClassModelManager.IsClassIgnoredForModeling(ClassDeclaration);

        Assert.That(IsIgnored, Is.False);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_RemoveNoClass()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_3
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: false, manager => RemoveClasses(manager, new List<ClassName>() { ClassName.FromSimpleString("Program_CoreClassModelManager_3") }));

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClassModelManager_3
"));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_RemoveAllClasses()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_4
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: false, manager => RemoveClasses(manager, new List<ClassName>()));

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClassModelManager_4
"));
    }

    [Test]
    [Category("Core")]
    public async Task ClassModelManager_DuplicateVerification()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_5
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        List<IClassModel> ClassModelList = await TestHelper.ToClassModelAsync(new List<ClassDeclarationSyntax>() { ClassDeclaration, ClassDeclaration }, TokenReplacement, manager => RemoveClasses(manager, new List<ClassName>() { ClassName.FromSimpleString("Program_CoreClassModelManager_5") }));
        Assert.That(ClassModelList.Count, Is.EqualTo(2));
        ClassModel ClassModel1 = (ClassModel)ClassModelList[0];
        ClassModel ClassModel2 = (ClassModel)ClassModelList[1];

        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel2.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_EmptyClassName()
    {
        ClassDeclarationSyntax ClassDeclaration = SyntaxFactory.ClassDeclaration(string.Empty);

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        Assert.Throws<ArgumentException>(() => TestHelper.ToClassModel(ClassDeclaration, TokenReplacement));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_EmptyClassNameAsync()
    {
        ClassDeclarationSyntax ClassDeclaration = SyntaxFactory.ClassDeclaration(string.Empty);

        bool IsIgnored = ClassModelManager.IsClassIgnoredForModeling(ClassDeclaration);

        Assert.That(IsIgnored, Is.True);

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        Assert.ThrowsAsync<ArgumentException>(() => TestHelper.ToClassModelAsync(ClassDeclaration, TokenReplacement));
    }

    [Test]
    [Category("Core")]
    public async Task ClassModelManager_DuplicateClassRemoved()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_6
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        List<IClassModel> ClassModelList = await TestHelper.ToClassModelAsync(new List<ClassDeclarationSyntax>() { ClassDeclaration, ClassDeclaration }, TokenReplacement, manager => RemoveClasses(manager, new List<ClassName>()));
        Assert.That(ClassModelList.Count, Is.EqualTo(2));
        ClassModel ClassModel1 = (ClassModel)ClassModelList[0];
        ClassModel ClassModel2 = (ClassModel)ClassModelList[1];

        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel2.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_DuplicateVerificationWithUpdate()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_7
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, DuplicateVerificationWithUpdate);
    }

    private void DuplicateVerificationWithUpdate(List<ClassDeclarationSyntax> classDeclarationList)
    {
        IClassModel ClassModel;
        MadeUpSemanticModel SemanticModel = new();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;
        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;

        Manager.GetVerifiedModel(ClassModel);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_VerificationWithErrorAndUpdate()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_8
{
    string X;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, VerificationWithErrorAndUpdate);
    }

    private void VerificationWithErrorAndUpdate(List<ClassDeclarationSyntax> classDeclarationList)
    {
        ClassDeclarationSyntax ClassDeclaration = classDeclarationList[0];
        IClassModel ClassModel;
        MadeUpSemanticModel SemanticModel = new();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);

        Manager.GetVerifiedModel(ClassModel);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithAutoStart()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_9
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, UpdateWithAutoStart);
    }

    private void UpdateWithAutoStart(List<ClassDeclarationSyntax> classDeclarationList)
    {
        IClassModel ClassModel;
        MadeUpSemanticModel SemanticModel = new();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Auto };

        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        Manager.GetVerifiedModel(ClassModel);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithBlockedClientChannel()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_10
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, UpdateWithBlockedClientChannel);
    }

    private void UpdateWithBlockedClientChannel(List<ClassDeclarationSyntax> classDeclarationList)
    {
        IClassModel ClassModel;
        MadeUpSemanticModel SemanticModel = new();

        Guid ServerToClientGuid = new Guid("{30AC0040-5412-4DF0-AAB3-28D66599E7D2}");

        using Channel Channel = new Channel(ServerToClientGuid, Mode.Receive);
        Channel.Open();

        using ClassModelManager Manager = new(ServerToClientGuid) { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        Manager.GetVerifiedModel(ClassModel);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithBlockedServerChannel()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_11
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, UpdateWithBlockedServerChannel);
    }

    private void UpdateWithBlockedServerChannel(List<ClassDeclarationSyntax> classDeclarationList)
    {
        IClassModel ClassModel;
        MadeUpSemanticModel SemanticModel = new();

        Guid ClientToServerGuid = ProcessCommunication.Channel.ClientToServerGuid;

        using Channel Channel = new Channel(ClientToServerGuid, Mode.Send);
        Channel.Open();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        Manager.GetVerifiedModel(ClassModel);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithBlockedServerProcess()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_12
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, UpdateWithBlockedServerProcess);
    }

    private void UpdateWithBlockedServerProcess(List<ClassDeclarationSyntax> classDeclarationList)
    {
        MadeUpSemanticModel SemanticModel = new();

        BlockServerProcess(out string ExtractPath);

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        IClassModel ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        Manager.GetVerifiedModel(ClassModel);

        RestoreServerProcess(ExtractPath);
    }

    private void BlockServerProcess(out string extractPath)
    {
        // Wait to be sure the verifier process exited.
        Thread.Sleep(Timeouts.VerificationIdleTimeout + Timeouts.VerificationIdleTimeout);

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };
        extractPath = Extractor.GetExtractedPath(Extractor.VerifierFileName);

        bool IsProcessReplaced = false;

        try
        {
            using FileStream Stream = new(extractPath, FileMode.Create, FileAccess.Write);
            using StreamWriter Writer = new(Stream);
            Writer.WriteLine("Replaced exe");

            IsProcessReplaced = true;
        }
        catch
        {
        }

        Assert.That(IsProcessReplaced, Is.True);
    }

    private void RestoreServerProcess(string extractPath)
    {
        try
        {
            // Delete the modified file so that next test will restore it.
            File.Delete(extractPath);
        }
        catch
        {
        }

        // Make the extractor will not assume it's already extracted.
        Extractor.Reset();
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithCorruptedResult()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_13
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, UpdateWithCorruptedResult);
    }

    private void UpdateWithCorruptedResult(List<ClassDeclarationSyntax> classDeclarationList)
    {
        IClassModel ClassModel;
        MadeUpSemanticModel SemanticModel = new();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        using Channel Channel = new Channel(Manager.ReceiveChannelGuid, Mode.Send);
        Channel.Open();

        Assert.That(Channel.IsOpen, Is.True);

        int EmptyPacketSize = sizeof(int);
        byte[] EmptyPacketBytes = BitConverter.GetBytes(EmptyPacketSize);
        Channel.Write(EmptyPacketBytes);

        Manager.GetVerifiedModel(ClassModel);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithClassDisappeared()
    {
        ClassDeclarationSyntax ClassDeclaration0 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_14
{
    int X;
}
// Invariant: X > 0
").First();

        ClassDeclarationSyntax ClassDeclaration1 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_15
{
    int X;
}
// Invariant: X > 0
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration0);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration0, ClassDeclaration1 }, TokenReplacement, UpdateWithClassDisappeared);
    }

    private void UpdateWithClassDisappeared(List<ClassDeclarationSyntax> classDeclarationList)
    {
        List<ClassDeclarationSyntax> ClassDeclarationList0 = new() { classDeclarationList[0] };
        List<ClassDeclarationSyntax> ClassDeclarationList1 = new() { classDeclarationList[1] };
        IClassModel ClassModel0;
        IClassModel ClassModel1;
        MadeUpSemanticModel SemanticModel = new();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Auto };

        CompilationContext CompilationContext = CompilationContext.GetAnother();
        ClassModel0 = Manager.GetClassModels(CompilationContext, ClassDeclarationList0, SemanticModel).First().Value;
        Manager.RemoveMissingClasses(new List<ClassName>());
        ClassModel1 = Manager.GetClassModels(CompilationContext, ClassDeclarationList1, SemanticModel).First().Value;

        Thread.Sleep(Timeouts.VerifierProcessLaunchTimeout);

        ClassModel0 = Manager.GetVerifiedModel(ClassModel0);
        ClassModel1 = Manager.GetVerifiedModel(ClassModel1);

        Assert.That(ClassModel0.InvariantViolations.Count, Is.EqualTo(0));
        Assert.That(ClassModel1.InvariantViolations.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithLittleCapacity()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_16
{
    int X;
}
// Invariant: X > 0
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, UpdateWithLittleCapacity);
    }

    private void UpdateWithLittleCapacity(List<ClassDeclarationSyntax> classDeclarationList)
    {
        IClassModel ClassModel;
        MadeUpSemanticModel SemanticModel = new();

        int OldCapacity = Channel.Capacity;
        Channel.Capacity = sizeof(int);

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        // Wait to be sure the verifier process exited. Otherwise, the different capacity will break the channel.
        Thread.Sleep(Timeouts.VerificationIdleTimeout + TimeSpan.FromSeconds(10));

        ClassModel = Manager.GetVerifiedModel(ClassModel);

        Channel.Capacity = OldCapacity;

        Assert.That(ClassModel.InvariantViolations.Count, Is.EqualTo(0));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_VerificationWithResult()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_17
{
    int X;

    public int Read()
    {
        X = 1;
        return X;
    }
    // Ensure: Result == 0
}
// Invariant: X == 0
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, VerificationWithResult);
    }

    private void VerificationWithResult(List<ClassDeclarationSyntax> classDeclarationList)
    {
        IClassModel ClassModel;
        MadeUpSemanticModel SemanticModel = new();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;
        ClassModel = Manager.GetVerifiedModel(ClassModel);

        Assert.That(ClassModel.EnsureViolations.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_VerificationWithContradictoryRequire()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_18
{
    int X;

    public void Write1(int x)
    {
        X = x;
    }

    public void Write2(int x)
    // Require: x == 0
    // Require: x != 0
    {
        X = x;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, VerificationWithContradictoryRequire);
    }

    private void VerificationWithContradictoryRequire(List<ClassDeclarationSyntax> classDeclarationList)
    {
        IClassModel ClassModel;
        MadeUpSemanticModel SemanticModel = new();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;
        ClassModel = Manager.GetVerifiedModel(ClassModel);

        Assert.That(ClassModel.RequireViolations.Count, Is.EqualTo(1));

        IRequireViolation RequireViolation = ClassModel.RequireViolations.First();
        Assert.That(RequireViolation.Method.Name.Text, Is.EqualTo("Write2"));
        Assert.That(RequireViolation.Text, Is.EqualTo("x != 0"));
        Assert.That(RequireViolation.Require, Is.Not.Null);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithInvariantDisappeared()
    {
        ClassDeclarationSyntax ClassDeclaration0 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_19
{
    int X;
}
// Invariant: X > 0
").First();

        ClassDeclarationSyntax ClassDeclaration1 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_19
{
    int X;
}
",
isClassNameRepeated: true).First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration0);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration0, ClassDeclaration1 }, TokenReplacement, UpdateWithInvariantDisappeared);
    }

    private void UpdateWithInvariantDisappeared(List<ClassDeclarationSyntax> classDeclarationList)
    {
        UpdateWithReadingDelayAdded(classDeclarationList, out IClassModel ClassModel);

        Assert.That(ClassModel.InvariantViolations.Count, Is.EqualTo(0));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithInvariantStillThere()
    {
        ClassDeclarationSyntax ClassDeclaration0 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_20
{
    int X;
    int Y;
}
").First();

        ClassDeclarationSyntax ClassDeclaration1 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_20
{
    int X;
    int Y;
}
// Invariant: X == 0
// Invariant: Y > 0
",
isClassNameRepeated: true).First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration0);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration0, ClassDeclaration1 }, TokenReplacement, UpdateWithInvariantStillThere);
    }

    private void UpdateWithInvariantStillThere(List<ClassDeclarationSyntax> classDeclarationList)
    {
        UpdateWithReadingUnchangedDelay(classDeclarationList, out IClassModel ClassModel);

        Assert.That(ClassModel.InvariantViolations.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithRequireDisappeared()
    {
        ClassDeclarationSyntax ClassDeclaration0 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_21
{
    int X;

    public void Write(int x)
    // Require: x == 0
    // Require: x != 0
    {
        X = x;
    }
}
").First();

        ClassDeclarationSyntax ClassDeclaration1 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_21
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}
",
isClassNameRepeated: true).First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration0);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration0, ClassDeclaration1 }, TokenReplacement, UpdateWithRequireDisappeared);
    }

    private void UpdateWithRequireDisappeared(List<ClassDeclarationSyntax> classDeclarationList)
    {
        UpdateWithReadingDelayAdded(classDeclarationList, out IClassModel ClassModel);

        Assert.That(ClassModel.RequireViolations.Count, Is.EqualTo(0));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithRequireStillThere()
    {
        ClassDeclarationSyntax ClassDeclaration0 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_22
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}
").First();

        ClassDeclarationSyntax ClassDeclaration1 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_22
{
    int X;

    public void Write(int x)
    // Require: x == 0
    // Require: x != 0
    {
        X = x;
    }
}
",
isClassNameRepeated: true).First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration0);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration0, ClassDeclaration1 }, TokenReplacement, UpdateWithRequireStillThere);
    }

    private void UpdateWithRequireStillThere(List<ClassDeclarationSyntax> classDeclarationList)
    {
        UpdateWithReadingUnchangedDelay(classDeclarationList, out IClassModel ClassModel);

        Assert.That(ClassModel.RequireViolations.Count, Is.EqualTo(1));

        IRequireViolation RequireViolation = ClassModel.RequireViolations.First();
        Assert.That(RequireViolation.Method.Name.Text, Is.EqualTo("Write"));
        Assert.That(RequireViolation.Text, Is.EqualTo("x != 0"));
        Assert.That(RequireViolation.Require, Is.Not.Null);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithEnsureDisappeared()
    {
        ClassDeclarationSyntax ClassDeclaration0 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_23
{
    public int X { get; set; }

    public void Write(int x)
    {
        X = x;
    }
    // Ensure: X == 0
}
").First();

        ClassDeclarationSyntax ClassDeclaration1 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_23
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}
",
isClassNameRepeated: true).First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration0);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration0, ClassDeclaration1 }, TokenReplacement, UpdateWithEnsureDisappeared);
    }

    private void UpdateWithEnsureDisappeared(List<ClassDeclarationSyntax> classDeclarationList)
    {
        UpdateWithReadingDelayAdded(classDeclarationList, out IClassModel ClassModel);

        Assert.That(ClassModel.EnsureViolations.Count, Is.EqualTo(0));
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_UpdateWithEnsureStillThere()
    {
        ClassDeclarationSyntax ClassDeclaration0 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_24
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}
").First();

        ClassDeclarationSyntax ClassDeclaration1 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_24
{
    public int X { get; set; }

    public void Write(int x)
    {
        X = x;
    }
    // Ensure: X == 0
}
",
isClassNameRepeated: true).First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration0);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration0, ClassDeclaration1 }, TokenReplacement, UpdateWithEnsureStillThere);
    }

    private void UpdateWithEnsureStillThere(List<ClassDeclarationSyntax> classDeclarationList)
    {
        UpdateWithReadingUnchangedDelay(classDeclarationList, out IClassModel ClassModel);

        Assert.That(ClassModel.EnsureViolations.Count, Is.EqualTo(1));
    }

    private void RemoveClasses(ClassModelManager manager, List<ClassName> existingClassNameList)
    {
        manager.RemoveMissingClasses(existingClassNameList);
    }

    private void UpdateWithReadingDelayAdded(List<ClassDeclarationSyntax> classDeclarationList, out IClassModel classModel)
    {
        UpdateWithReadingDelayChange(classDeclarationList, isVerificationSlow: true, out classModel);
    }

    private void UpdateWithReadingUnchangedDelay(List<ClassDeclarationSyntax> classDeclarationList, out IClassModel classModel)
    {
        UpdateWithReadingDelayChange(classDeclarationList, isVerificationSlow: false, out classModel);
    }

    private void UpdateWithReadingDelayChange(List<ClassDeclarationSyntax> classDeclarationList, bool isVerificationSlow, out IClassModel classModel)
    {
        TimeSpan OldDelay = ClassModelManager.DelayBeforeReadingVerificationResult;

        if (isVerificationSlow)
            ClassModelManager.DelayBeforeReadingVerificationResult = TimeSpan.FromSeconds(10);

        List<ClassDeclarationSyntax> ClassDeclarationList0 = new() { classDeclarationList[0] };
        List<ClassDeclarationSyntax> ClassDeclarationList1 = new() { classDeclarationList[1] };
        IClassModel ClassModel0;
        IClassModel ClassModel1;
        MadeUpSemanticModel SemanticModel = new();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel0 = Manager.GetClassModels(CompilationContext.GetAnother(), ClassDeclarationList0, SemanticModel).First().Value;
        Task<IClassModel> GetClassModelTask0 = Manager.GetVerifiedModelAsync(ClassModel0);

        if (isVerificationSlow)
        {
            SynchronizedVerificationContext VerificationContext = (SynchronizedVerificationContext)Manager.GetType().GetField("Context", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(Manager)!;
            while (!VerificationContext.VerificationState.IsVerificationRequestSent)
                GetClassModelTask0.Wait(TimeSpan.FromMilliseconds(10));
        }
        else
        {
            GetClassModelTask0.Wait(TimeSpan.FromSeconds(10));
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        ClassModel1 = Manager.GetClassModels(CompilationContext.GetAnother(), ClassDeclarationList1, SemanticModel).First().Value;
        Task<IClassModel> GetClassModelTask1 = Manager.GetVerifiedModelAsync(ClassModel1);

        GetClassModelTask0.Wait();
        GetClassModelTask1.Wait();

        ClassModel0 = GetClassModelTask0.Result;
        ClassModel1 = GetClassModelTask1.Result;

        ClassModelManager.DelayBeforeReadingVerificationResult = OldDelay;

        classModel = ClassModel1;
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_RequireViolationInPrivateStaticFunction()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_25
{
    static double NotUsed(double d)
    {
        return d;
    }

    static double Write(double d)
    // Require: d >= 0
    {
        return d;
    }
    // Ensure: Result >= 0

    public double TestWrite(double d)
    {
        return Write(d);
    }
    // Ensure: Result >= 0
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, RequireViolationInPrivateStaticFunction);
    }

    private void RequireViolationInPrivateStaticFunction(List<ClassDeclarationSyntax> classDeclarationList)
    {
        IClassModel ClassModel;
        MadeUpSemanticModel SemanticModel = new();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;
        ClassModel = Manager.GetVerifiedModel(ClassModel);

        Assert.That(ClassModel.RequireViolations.Count, Is.EqualTo(1));

        IRequireViolation RequireViolation = ClassModel.RequireViolations.First();
        Assert.That(RequireViolation.Method.Name.Text, Is.EqualTo("Write"));
        Assert.That(RequireViolation.Text, Is.EqualTo("d >= 0"));
        Assert.That(RequireViolation.Expression, Is.Not.Null);
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_RequireViolationInPrivateStaticMethod()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClassModelManager_26
{
    static void NotUsed(double d)
    {
    }

    static void Write(double d)
    // Require: d >= 0
    {
    }

    public void TestWrite(double d)
    {
        Write(d);
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        TestHelper.ExecuteClassModelTest(new List<ClassDeclarationSyntax>() { ClassDeclaration }, TokenReplacement, RequireViolationInPrivateStaticMethod);
    }

    private void RequireViolationInPrivateStaticMethod(List<ClassDeclarationSyntax> classDeclarationList)
    {
        IClassModel ClassModel;
        MadeUpSemanticModel SemanticModel = new();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = VerificationProcessStartMode.Manual };

        ClassModel = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel).First().Value;
        ClassModel = Manager.GetVerifiedModel(ClassModel);

        Assert.That(ClassModel.RequireViolations.Count, Is.EqualTo(1));

        IRequireViolation RequireViolation = ClassModel.RequireViolations.First();
        Assert.That(RequireViolation.Method.Name.Text, Is.EqualTo("Write"));
        Assert.That(RequireViolation.Text, Is.EqualTo("d >= 0"));
        Assert.That(RequireViolation.Statement, Is.Not.Null);
    }
}
