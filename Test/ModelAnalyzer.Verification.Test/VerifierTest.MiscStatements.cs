namespace ModelAnalyzer.Verification.Test;

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
    private const string SourceCode1 = @"
using System;

class Program_Verifier_MiscStatement1
{
    int X;

    void Write()
    {
        X = X + 1;

        if (X == 1)
        {
            X = X * 1;

            if (X == 1)
            {
                X = X - 1;
            }
            else
                X = 1;
        }
        else
            X = 1;
    }
}
// Invariant: X == 0
";

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(SourceCode1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private Verifier CreateVerifierFromSourceCode(string sourceCode, int maxDepth)
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

        Verifier TestObject = new()
        {
            ClassName = ClassModel.Name,
            FieldTable = ClassModel.FieldTable,
            MethodTable = ClassModel.MethodTable,
            InvariantList = ClassModel.InvariantList,
            MaxDepth = maxDepth,
        };

        return TestObject;
    }
}
