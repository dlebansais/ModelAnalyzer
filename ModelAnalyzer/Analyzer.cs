namespace ModelAnalyzer;

using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using AnalysisLogger;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

public abstract class Analyzer : DiagnosticAnalyzer
{
    protected IAnalysisLogger Logger { get; } = Initialization.Logger;
    protected ClassModelManager Manager { get; } = Initialization.Manager;
    
    protected abstract string Id { get; }
    protected abstract SyntaxKind DiagnosticKind { get; }
    protected abstract bool IsAsyncRunRequested { get; }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        switch (DiagnosticKind)
        {
            case SyntaxKind.ClassDeclaration:
                context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, DiagnosticKind);
                break;
            case SyntaxKind.CompilationUnit:
                context.RegisterSyntaxNodeAction(AnalyzeCompilationUnit, DiagnosticKind);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        try
        {
            var ClassDeclaration = (ClassDeclarationSyntax)context.Node;
            AnalyzeClass(context, ClassDeclaration);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }

    private void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
    {
        try
        {
            var CompilationUnit = (CompilationUnitSyntax)context.Node;

            foreach (MemberDeclarationSyntax Member in CompilationUnit.Members)
                if (Member is FileScopedNamespaceDeclarationSyntax FileScopedNamespaceDeclaration)
                    AnalyzeNamespaceMembers(context, FileScopedNamespaceDeclaration.Members);
                else if (Member is NamespaceDeclarationSyntax NamespaceDeclaration)
                    AnalyzeNamespaceMembers(context, NamespaceDeclaration.Members);
                else if (Member is ClassDeclarationSyntax ClassDeclaration)
                    AnalyzeClass(context, ClassDeclaration);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }

    private void AnalyzeNamespaceMembers(SyntaxNodeAnalysisContext context, SyntaxList<MemberDeclarationSyntax> members)
    {
        List<string> ExistingClassList = new();

        foreach (MemberDeclarationSyntax NamespaceMember in members)
            if (NamespaceMember is ClassDeclarationSyntax ClassDeclaration)
            {
                ExistingClassList.Add(ClassDeclaration.Identifier.ValueText);
                AnalyzeClass(context, ClassDeclaration);
            }

        Manager.RemoveMissingClasses(ExistingClassList);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
    {
        // Ignore diagnostic for classes not modeled.
        if (ClassModelManager.IsClassIgnoredForModeling(classDeclaration))
            return;

        CompilationContext CompilationContext = CompilationContextHelper.ToCompilationContext(Id, classDeclaration, isAsyncRunRequested: IsAsyncRunRequested);
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext, classDeclaration);

        ReportDiagnostic(context, classDeclaration, ClassModel);
    }

    protected abstract void ReportDiagnostic(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IClassModel classModel);
}
