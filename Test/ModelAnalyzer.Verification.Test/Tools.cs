namespace Verification.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelAnalyzer;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public static class Tools
{
    internal static Verifier CreateVerifierFromSourceCode(string sourceCode, int maxDepth, TimeSpan maxDuration)
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

    internal static List<Verifier> CreateMultiVerifierFromSourceCode(string sourceCode, int maxDepth, TimeSpan maxDuration)
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

        List<Verifier> TestObjectList = new();

        foreach (KeyValuePair<ClassDeclarationSyntax, IClassModel> Entry in ClassModels)
        {
            ClassModel ClassModel = (ClassModel)Entry.Value;

            Verifier TestObject = new()
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

            TestObjectList.Add(TestObject);
        }

        return TestObjectList;
    }
}
