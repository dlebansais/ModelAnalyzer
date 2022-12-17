namespace ModelAnalyzer.Core.Test;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

#if REMOVED
public class SynchronizedThreadTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        SynchronizedVerificationContext Context = new();
        bool IsBasicTestOk = false;

        using (SynchronizedThread<ModelVerification, ClassModel> SynchronizedThread = new(Context, (IDictionary<ModelVerification, ClassModel> table) =>
        {
            IsBasicTestOk = true;
        }))
        {
            Assert.That(SynchronizedThread.Context, Is.EqualTo(Context));
            Assert.That(SynchronizedThread.Callback, Is.Not.Null);
            SynchronizedThread.Start();
        }

        Assert.IsTrue(IsBasicTestOk);
    }

    [Test]
    [Category("Core")]
    public void SynchronizedThreadTest_ScheduleRestart()
    {
        SynchronizedVerificationContext Context = new();
        AutoResetEvent ThreadStartedEvent = new(initialState: false);
        ManualResetEvent UnlockThreadEvent = new(initialState: false);
        int ThreadExecutionCount = 0;

        using (SynchronizedThread<ModelVerification, ClassModel> SynchronizedThread = new(Context, (IDictionary<ModelVerification, ClassModel> table) =>
        {
            ThreadExecutionCount++;
            ThreadStartedEvent.Set();
            UnlockThreadEvent.WaitOne();
        }))
        {
            SynchronizedThread.Start();
            ThreadStartedEvent.WaitOne();

            Assert.That(ThreadExecutionCount, Is.EqualTo(1));

            SynchronizedThread.Start(); // Since the thread is on hold waiting for UnlockThreadEvent to be set, we know this will schedule a restart.
            UnlockThreadEvent.Set();

            ThreadStartedEvent.WaitOne();
        }

        Assert.That(ThreadExecutionCount, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void SynchronizedThreadTest_SingleClassModel()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreSynchronizedThread_0
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        ClassModel ClassModel = (ClassModel)TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        SynchronizedVerificationContext Context = new();
        int TableCount = 0;

        Context.UpdateClassModel(ClassModel, out bool IsAdded);

        Assert.IsTrue(IsAdded);
        Assert.That(Context.ClassModelTable.Count, Is.EqualTo(1));

        ModelVerification ModelVerification = new() { ClassModel = ClassModel };
        Context.VerificationList.Add(ModelVerification);
        Assert.That(Context.VerificationList.Count, Is.EqualTo(1));

        using (SynchronizedThread<ModelVerification, ClassModel> SynchronizedThread = new(Context, (IDictionary<ModelVerification, ClassModel> table) =>
        {
            TableCount = table.Count;
        }))
        {
            SynchronizedThread.Start();
        }

        Assert.That(TableCount, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void SynchronizedThreadTest_FindClassModel()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreSynchronizedThread_1
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        ClassModel ClassModel = (ClassModel)TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        SynchronizedVerificationContext Context = new();

        Context.UpdateClassModel(ClassModel, out bool IsAdded);

        ModelVerification? ModelVerification;
        ModelVerification = Context.FindByName("Program_CoreSynchronizedThread_1");

        Assert.That(ModelVerification, Is.Null);

        ModelVerification NewModelVerification = new() { ClassModel = ClassModel };
        Context.VerificationList.Add(NewModelVerification);

        ModelVerification = Context.FindByName("Program_CoreSynchronizedThread_1");

        Assert.That(ModelVerification, Is.Not.Null);
    }

    [Test]
    [Category("Core")]
    public void SynchronizedThreadTest_OneClassModelTwoVerifiers()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreSynchronizedThread_2
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        ClassModel ClassModel = (ClassModel)TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        SynchronizedVerificationContext Context = new();
        int TableCount = 0;

        Context.UpdateClassModel(ClassModel, out bool IsAdded);
        Assert.IsTrue(IsAdded);
        Assert.That(Context.ClassModelTable.Count, Is.EqualTo(1));

        Context.UpdateClassModel(ClassModel, out IsAdded);
        Assert.IsFalse(IsAdded);
        Assert.That(Context.ClassModelTable.Count, Is.EqualTo(1));

        ModelVerification ModelVerification1 = new() { ClassModel = ClassModel };
        ModelVerification ModelVerification2 = new() { ClassModel = ClassModel };
        Context.VerificationList.Add(ModelVerification1);
        Context.VerificationList.Add(ModelVerification2);
        Assert.That(Context.VerificationList.Count, Is.EqualTo(2));

        using (SynchronizedThread<ModelVerification, ClassModel> SynchronizedThread = new(Context, (IDictionary<ModelVerification, ClassModel> table) =>
        {
            TableCount = table.Count;
        }))
        {
            SynchronizedThread.Start();
        }

        Assert.That(TableCount, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public async Task SynchronizedThreadTest_TwoClassModels()
    {
        ClassDeclarationSyntax ClassDeclaration1 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreSynchronizedThread_3
{
}
");

        ClassDeclarationSyntax ClassDeclaration2 = TestHelper.FromSourceCode(@"
using System;

class Program_CoreSynchronizedThread_4
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration1);

        SynchronizedVerificationContext Context = new();
        int TableCount = 0;
        bool IsAdded;

        List<IClassModel> ClassModelList = await TestHelper.ToClassModelAsync(new List<ClassDeclarationSyntax>() { ClassDeclaration1, ClassDeclaration2 }, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));
        ClassModel ClassModel1 = (ClassModel)ClassModelList[0];
        ClassModel ClassModel2 = (ClassModel)ClassModelList[1];

        Context.UpdateClassModel(ClassModel1, out IsAdded);
        Assert.IsTrue(IsAdded);
        Assert.That(Context.ClassModelTable.Count, Is.EqualTo(1));

        ModelVerification ModelVerification1 = new() { ClassModel = ClassModel1 };
        Context.VerificationList.Add(ModelVerification1);
        Assert.That(Context.VerificationList.Count, Is.EqualTo(1));

        Context.UpdateClassModel(ClassModel2, out IsAdded);
        Assert.IsTrue(IsAdded);
        Assert.That(Context.ClassModelTable.Count, Is.EqualTo(2));

        ModelVerification ModelVerification2 = new() { ClassModel = ClassModel2 };
        Context.VerificationList.Add(ModelVerification2);
        Assert.That(Context.VerificationList.Count, Is.EqualTo(2));

        using (SynchronizedThread<ModelVerification, ClassModel> SynchronizedThread = new(Context, (IDictionary<ModelVerification, ClassModel> table) =>
        {
            TableCount = table.Count;
        }))
        {
            SynchronizedThread.Start();
        }

        Assert.That(TableCount, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void SynchronizedThreadTest_Dispose()
    {
        using (SynchronizedThreadExtended TestObject = new SynchronizedThreadExtended())
        {
        }
    }

    [Test]
    [Category("Core")]
    public void SynchronizedThreadTest_DoubleDispose()
    {
        using (SynchronizedThreadExtended TestObject = new SynchronizedThreadExtended())
        {
            TestObject.Dispose();
        }
    }

    [Test]
    [Category("Core")]
    public void SynchronizedThreadTest_FakeFinalize()
    {
        using (SynchronizedThreadExtended TestObject = new SynchronizedThreadExtended())
        {
            TestObject.FakeFinalize();
        }
    }

    [Test]
    [Category("Core")]
    public void SynchronizedThreadTest_Destructor()
    {
        using SynchronizedThreadContainer Container = new();
    }
}
#endif