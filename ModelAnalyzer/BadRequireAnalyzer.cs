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
public class BadRequireAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Design";

    public const string BadRequireDiagnosticId = "BadRequire";
    private static readonly LocalizableString BadRequireTitle = new LocalizableResourceString(nameof(Resources.BadRequireAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadRequireMessageFormat = new LocalizableResourceString(nameof(Resources.BadRequireAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadRequireDescription = new LocalizableResourceString(nameof(Resources.BadRequireAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor BadRequireRule = new DiagnosticDescriptor(BadRequireDiagnosticId, BadRequireTitle, BadRequireMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: BadRequireDescription);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(BadRequireRule);
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

        CompilationContext CompilationContext = CompilationContextHelper.ToCompilationContext(context, isAsyncRunRequested: false);
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext, classDeclaration);
        string ClassName = classDeclaration.Identifier.ValueText;

        foreach (IUnsupportedRequire Item in ClassModel.Unsupported.Requires)
        {
            Logger.Log(LogLevel.Warning, $"Class '{ClassName}': reporting bad require.");
            context.ReportDiagnostic(Diagnostic.Create(BadRequireRule, Item.Location));
        }
    }
}
