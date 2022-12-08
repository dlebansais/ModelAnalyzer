namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Reflection;
using System;

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

    public static void ReplaceTokenKind(SyntaxToken token, SyntaxKind newKind, out SyntaxKind oldKind)
    {
        Type TokenType = typeof(SyntaxToken);
        PropertyInfo NodeProperty = TokenType.GetProperty("Node", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo KindField = NodeProperty.PropertyType.GetField("_kind", BindingFlags.Instance | BindingFlags.NonPublic)!;

        object Node = NodeProperty.GetValue(token)!;
        oldKind = (SyntaxKind)(ushort)KindField.GetValue(Node)!;
        KindField.SetValue(Node, newKind);
    }
}
