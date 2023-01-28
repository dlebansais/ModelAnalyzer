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
    public void ClassModelManager_Dispose()
    {
        using (ClassModelManagerExtended TestObject = new ClassModelManagerExtended())
        {
        }
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_DoubleDispose()
    {
        using (ClassModelManagerExtended TestObject = new ClassModelManagerExtended())
        {
            TestObject.Dispose();
        }
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_FakeFinalize()
    {
        using (ClassModelManagerExtended TestObject = new ClassModelManagerExtended())
        {
            TestObject.FakeFinalize();
        }
    }

    [Test]
    [Category("Core")]
    public void ClassModelManager_Destructor()
    {
        using ClassModelManagerContainer Container = new();
    }
}
