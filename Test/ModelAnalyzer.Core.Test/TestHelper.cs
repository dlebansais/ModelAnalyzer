namespace ModelAnalyzer.Core.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal class TestHelper
{
    private static List<string> ClassNameList = new();

    public static ClassDeclarationSyntax FromSourceCode(string sourceCode)
    {
        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText(sourceCode, Options);
        var Diagnostics = SyntaxTree.GetDiagnostics();
        List<Diagnostic> ErrorList = new();

        foreach (Diagnostic Diagnostic in Diagnostics)
            if (Diagnostic.Severity == DiagnosticSeverity.Error && Diagnostic.Id != "CS1029")
                ErrorList.Add(Diagnostic);

        Debug.Assert(ErrorList.Count == 0);

        CompilationUnitSyntax Root = SyntaxTree.GetCompilationUnitRoot();

        Debug.Assert(Root.Members.Count == 1);

        ClassDeclarationSyntax ClassDeclaration;
        if (Root.Members[0] is ClassDeclarationSyntax AsClassDeclaration)
            ClassDeclaration = AsClassDeclaration;
        else
            ClassDeclaration = (ClassDeclarationSyntax)((NamespaceDeclarationSyntax)Root.Members[0]).Members[0];

        string ClassName = ClassDeclaration.Identifier.ValueText;

        Debug.Assert(!ClassNameList.Contains(ClassName));
        ClassNameList.Add(ClassName);

        return ClassDeclaration;
    }

    public static TokenReplacement BeginReplaceToken(ClassDeclarationSyntax classDeclaration)
    {
        return BeginReplaceToken(classDeclaration, classDeclaration => default, SyntaxKind.None);
    }

    public static TokenReplacement BeginReplaceToken(ClassDeclarationSyntax classDeclaration, Func<ClassDeclarationSyntax, SyntaxToken> locator, SyntaxKind newKind)
    {
        TokenReplacement Result;
        SyntaxToken Token = locator(classDeclaration);

        if (Token != default)
            Result = new TokenReplacement(Token, newKind);
        else
            Result = TokenReplacement.None;

        return Result;
    }

    public static IClassModel ToClassModel(ClassDeclarationSyntax classDeclaration, TokenReplacement tokenReplacement, bool waitIfAsync = false)
    {
        tokenReplacement.Replace();

        ClassModelManager Manager = new();
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext.GetAnother(), classDeclaration, waitIfAsync);

        tokenReplacement.Restore();

        return ClassModel;
    }

    public static async Task<IClassModel> ToClassModelAsync(ClassDeclarationSyntax classDeclaration, TokenReplacement tokenReplacement)
    {
        tokenReplacement.Replace();

        ClassModelManager Manager = new();
        IClassModel ClassModel = await Manager.GetClassModelAsync(CompilationContext.GetAnother(), classDeclaration);

        tokenReplacement.Restore();

        return ClassModel;
    }
}
