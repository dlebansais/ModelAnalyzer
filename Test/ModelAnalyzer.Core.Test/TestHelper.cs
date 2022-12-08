namespace ModelAnalyzer.Core.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal class TestHelper
{
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
        ClassDeclarationSyntax ClassDeclaration = (ClassDeclarationSyntax)Root.Members[0];

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

    public static IClassModel ToClassModel(ClassDeclarationSyntax classDeclaration, TokenReplacement tokenReplacement)
    {
        tokenReplacement.Replace();

        ClassModelManager Manager = new();
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext.Default, classDeclaration);

        tokenReplacement.Restore();

        return ClassModel;
    }
}
