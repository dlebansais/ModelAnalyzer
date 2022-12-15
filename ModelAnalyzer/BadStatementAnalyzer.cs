﻿namespace ModelAnalyzer;

using System;
using System.Collections.Immutable;
using AnalysisLogger;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BadStatementAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Design";

    public const string DiagnosticId = "MA0006";
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.BadStatementAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.BadStatementAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.BadStatementAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(Rule);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private IAnalysisLogger Logger = Initialization.Logger;
    private ClassModelManager Manager = Initialization.Manager;

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        try
        {
            var ClassDeclaration = (ClassDeclarationSyntax)context.Node;
            AnalyzeClass(context, ClassDeclaration);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
    {
        // Ignore diagnostic for classes not modeled.
        if (ClassModelManager.IsClassIgnoredForModeling(classDeclaration))
            return;

        CompilationContext CompilationContext = CompilationContextHelper.ToCompilationContext(DiagnosticId, classDeclaration, isAsyncRunRequested: false);
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext, classDeclaration);
        string ClassName = classDeclaration.Identifier.ValueText;

        foreach (IUnsupportedStatement Item in ClassModel.Unsupported.Statements)
        {
            Logger.Log(LogLevel.Warning, $"Class '{ClassName}': reporting bad statement.");
            context.ReportDiagnostic(Diagnostic.Create(Rule, Item.Location));
        }
    }
}
