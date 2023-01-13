namespace Verification.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    [Test]
    [Category("Verification")]
    public void Verifier_BasicTest()
    {
        string ClassName = "Test";

        ClassModel ClassModel = new ClassModel()
        {
            Name = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = ReadOnlyFieldTable.Empty,
            MethodTable = ReadOnlyMethodTable.Empty,
            InvariantList = new List<Invariant>().AsReadOnly(),
            Unsupported = new Unsupported(),
            InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
            RequireViolations = new List<IRequireViolation>().AsReadOnly(),
            EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
            AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
        };

        Verifier TestObject = new()
        {
            MaxDepth = 0,
            MaxDuration = TimeSpan.MaxValue,
            ClassModelTable = new Dictionary<string, ClassModel>() { { ClassName, ClassModel } },
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = ReadOnlyFieldTable.Empty,
            MethodTable = ReadOnlyMethodTable.Empty,
            InvariantList = new List<Invariant>().AsReadOnly(),
        };

        Assert.That(TestObject.ClassName, Is.EqualTo(ClassName));

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EmptyDepth1()
    {
        string ClassName = "Test";

        ClassModel ClassModel = new ClassModel()
        {
            Name = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = ReadOnlyFieldTable.Empty,
            MethodTable = ReadOnlyMethodTable.Empty,
            InvariantList = new List<Invariant>().AsReadOnly(),
            Unsupported = new Unsupported(),
            InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
            RequireViolations = new List<IRequireViolation>().AsReadOnly(),
            EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
            AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
        };

        Verifier TestObject = new()
        {
            MaxDepth = 1,
            MaxDuration = TimeSpan.MaxValue,
            ClassModelTable = new Dictionary<string, ClassModel>() { { ClassName, ClassModel } },
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = ReadOnlyFieldTable.Empty,
            MethodTable = ReadOnlyMethodTable.Empty,
            InvariantList = new List<Invariant>().AsReadOnly(),
        };

        Assert.That(TestObject.ClassName, Is.EqualTo(ClassName));

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ZeroDuration()
    {
        string SimpleClass = @"
using System;

class Program_Verifier_ZeroDuration
{
    public void Write()
    {
    }
}
";

        Verifier TestObject = Tools.CreateVerifierFromSourceCode(SimpleClass, maxDepth: 10, maxDuration: TimeSpan.Zero);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.Timeout));
    }
}
