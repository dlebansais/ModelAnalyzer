﻿namespace ModelAnalyzer.Core.Test;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public class SynchronizationThreadTest
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
    public void ScheduleRestartTest()
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
    public void ClassModelTest_SingleClassModel()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreSynchronizedThread_0
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        ClassModel ClassModel = (ClassModel)TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

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
    public void ClassModelTest_FindClassModel()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreSynchronizedThread_1
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        ClassModel ClassModel = (ClassModel)TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

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
    public void ClassModelTest_OneClassModelTwoVerifiers()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreSynchronizedThread_2
{
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        ClassModel ClassModel = (ClassModel)TestHelper.ToClassModel(ClassDeclaration, TokenReplacement, waitIfAsync: true);

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
}
