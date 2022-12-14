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
            MaxDuration = MaxDuration,
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
            MaxDuration = MaxDuration,
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
        MadeUpSemanticModel SemanticModel = new();

        List<ClassDeclarationSyntax> ClassDeclarationList = new();
        if (Root.Members[0] is ClassDeclarationSyntax AsClassDeclaration)
        {
            foreach (ClassDeclarationSyntax ClassDeclaration in Root.Members)
                ClassDeclarationList.Add(ClassDeclaration);
        }
        else
        {
            foreach (ClassDeclarationSyntax ClassDeclaration in ((NamespaceDeclarationSyntax)Root.Members[0]).Members)
                ClassDeclarationList.Add(ClassDeclaration);
        }

        using ClassModelManager Manager = new() { StartMode = VerificationProcessStartMode.Manual };
        IDictionary<ClassDeclarationSyntax, IClassModel> ClassModels = Manager.GetClassModels(CompilationContext.GetAnother(), ClassDeclarationList, SemanticModel);

        Debug.Assert(ClassModels.Count > 0);

        Dictionary<string, ClassModel> ClassModelTable = new();

        foreach (KeyValuePair<ClassDeclarationSyntax, IClassModel> Entry in ClassModels)
        {
            ClassModel ClassModel = (ClassModel)Entry.Value;

            Debug.Assert(ClassModel.Unsupported.IsEmpty);
            ClassModelTable.Add(ClassModel.Name, ClassModel);
        }

        Verifier TestObject = null!;

        foreach (KeyValuePair<ClassDeclarationSyntax, IClassModel> Entry in ClassModels)
        {
            ClassModel ClassModel = (ClassModel)Entry.Value;

            TestObject = new()
            {
                MaxDepth = maxDepth,
                MaxDuration = maxDuration,
                ClassModelTable = ClassModelTable,
                ClassName = ClassModel.Name,
                PropertyTable = ClassModel.PropertyTable,
                FieldTable = ClassModel.FieldTable,
                MethodTable = ClassModel.MethodTable,
                InvariantList = ClassModel.InvariantList,
            };
        }

        return TestObject;
    }
}
