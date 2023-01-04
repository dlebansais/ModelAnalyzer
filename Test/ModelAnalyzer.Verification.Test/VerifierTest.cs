namespace ModelAnalyzer.Verification.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        Verifier TestObject = new()
        {
            MaxDepth = 0,
            MaxDuration = MaxDuration,
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
        Verifier TestObject = new()
        {
            MaxDepth = 1,
            MaxDuration = MaxDuration,
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

        Verifier TestObject = CreateVerifierFromSourceCode(SimpleClass, maxDepth: 10, maxDuration: TimeSpan.Zero);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.Timeout));
    }

    private Verifier CreateVerifierFromSourceCode(string sourceCode, int maxDepth, TimeSpan maxDuration)
    {
        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText(sourceCode, Options);
        var Diagnostics = SyntaxTree.GetDiagnostics();
        List<Diagnostic> ErrorList = Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id != "CS1029").ToList();
        Debug.Assert(ErrorList.Count == 0);

        CompilationUnitSyntax Root = SyntaxTree.GetCompilationUnitRoot();

        Debug.Assert(Root.Members.Count == 1);

        ClassDeclarationSyntax ClassDeclaration;
        if (Root.Members[0] is ClassDeclarationSyntax AsClassDeclaration)
            ClassDeclaration = AsClassDeclaration;
        else
            ClassDeclaration = (ClassDeclarationSyntax)((NamespaceDeclarationSyntax)Root.Members[0]).Members[0];

        using ClassModelManager Manager = new() { StartMode = VerificationProcessStartMode.Manual };
        ClassModel ClassModel = (ClassModel)Manager.GetClassModel(CompilationContext.GetAnother(), ClassDeclaration);

        Debug.Assert(ClassModel.Unsupported.IsEmpty);

        Verifier TestObject = new()
        {
            MaxDepth = maxDepth,
            MaxDuration = maxDuration,
            ClassName = ClassModel.Name,
            PropertyTable = ClassModel.PropertyTable,
            FieldTable = ClassModel.FieldTable,
            MethodTable = ClassModel.MethodTable,
            InvariantList = ClassModel.InvariantList,
        };

        return TestObject;
    }
}
