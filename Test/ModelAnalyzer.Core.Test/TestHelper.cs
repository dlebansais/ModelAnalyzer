﻿namespace ModelAnalyzer.Core.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    public static IClassModel ToClassModel(ClassDeclarationSyntax classDeclaration, TokenReplacement tokenReplacement, bool waitIfAsync = false, Action<ClassModelManager>? managerHandler = null)
    {
        List<IClassModel> ClassModelList = ToClassModel(new List<ClassDeclarationSyntax>() { classDeclaration }, tokenReplacement, waitIfAsync, managerHandler);
        return ClassModelList.First();
    }

    public static List<IClassModel> ToClassModel(List<ClassDeclarationSyntax> classDeclarationList, TokenReplacement tokenReplacement, bool waitIfAsync = false, Action<ClassModelManager>? managerHandler = null)
    {
        tokenReplacement.Replace();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = classDeclarationList.Count > 1 ? SynchronizedThreadStartMode.Manual : SynchronizedThreadStartMode.Auto };

        List<IClassModel> ClassModelList = new();

        foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
            ClassModelList.Add(Manager.GetClassModel(CompilationContext.GetAnother(), ClassDeclaration, waitIfAsync));

        if (managerHandler is not null)
            managerHandler(Manager);

        if (Manager.StartMode == SynchronizedThreadStartMode.Manual)
            Manager.StartVerification();

        tokenReplacement.Restore();

        Manager.RemoveMissingClasses(new List<string>());

        return ClassModelList;
    }

    public static async Task<IClassModel> ToClassModelAsync(ClassDeclarationSyntax classDeclaration, TokenReplacement tokenReplacement, Action<ClassModelManager>? managerHandler = null)
    {
        List<IClassModel> ClassModelList = await ToClassModelAsync(new List<ClassDeclarationSyntax>() { classDeclaration }, tokenReplacement, managerHandler);
        return ClassModelList.First();
    }

    public static async Task<List<IClassModel>> ToClassModelAsync(List<ClassDeclarationSyntax> classDeclarationList, TokenReplacement tokenReplacement, Action<ClassModelManager>? managerHandler = null)
    {
        tokenReplacement.Replace();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = classDeclarationList.Count > 1 ? SynchronizedThreadStartMode.Manual : SynchronizedThreadStartMode.Auto };

        List<Task<IClassModel>> TaskList = new();
        CompilationContext CompilationContext = CompilationContext.Default;
        ClassDeclarationSyntax? PreviousClassDeclaration = null;

        foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
        {
            if (PreviousClassDeclaration != ClassDeclaration)
                CompilationContext = CompilationContext.GetAnother();

            TaskList.Add(Manager.GetClassModelAsync(CompilationContext, ClassDeclaration));

            PreviousClassDeclaration = ClassDeclaration;
        }

        if (managerHandler is not null)
            managerHandler(Manager);

        if (Manager.StartMode == SynchronizedThreadStartMode.Manual)
            Manager.StartVerification();

        List<IClassModel> ClassModelList = await Task.Run(async () =>
        {
            List<IClassModel> ClassModelList = new();

            foreach (Task<IClassModel> Task in TaskList)
                ClassModelList.Add(await Task);

            return ClassModelList;
        });

        tokenReplacement.Restore();

        return ClassModelList;
    }

    public static void ExecuteClassModelTest(List<ClassDeclarationSyntax> classDeclarationList, TokenReplacement tokenReplacement, Action<List<ClassDeclarationSyntax>> handler)
    {
        tokenReplacement.Replace();

        handler(classDeclarationList);

        tokenReplacement.Restore();
    }
}
