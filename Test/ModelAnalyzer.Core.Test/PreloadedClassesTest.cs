namespace Core.Test;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Miscellaneous.Test;
using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for preloaded classes.
/// </summary>
public class PreloadedClassesTest
{
    [Test]
    [Category("Core")]
    public void Preloaded_Sqrt()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;
using Math;

class Preloaded_Math_Sqrt_Test_Success
{
    public double TestSqrt(double d)
    // Require: d >= 0
    {
        return Math.Sqrt(d);
    }
    // Ensure: Result >= 0
    // Ensure: Result * Result == d
}

class Preloaded_Math_Sqrt_Test_Error1
{
    public double TestSqrt(double d)
    // Require: d >= 0
    {
        return Math.Sqrt(d);
    }
    // Ensure: Result >= 0
    // Ensure: Result * Result != d
}

class Preloaded_Math_Sqrt_Test_Error2
{
    public double TestSqrt(double d)
    {
        return Math.Sqrt(d);
    }
    // Ensure: Result >= 0
    // Ensure: Result * Result == d
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        // Wait for the verifier to finish other classes. TODO: do better than a sleep...
        Thread.Sleep(TimeSpan.FromMinutes(1));

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement, waitIfAsync: true);

        Assert.That(ClassModelList.Count, Is.EqualTo(3));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];
        IClassModel ClassModel2 = ClassModelList[2];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel0.EnsureViolations.Count, Is.EqualTo(0));

        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.EnsureViolations.Count, Is.EqualTo(1));

        // TODO: report the error at the call site, no at the called function (in this case, Math.Sqrt)
        Assert.That(ClassModel2.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel2.RequireViolations.Count, Is.EqualTo(0));
    }
}
