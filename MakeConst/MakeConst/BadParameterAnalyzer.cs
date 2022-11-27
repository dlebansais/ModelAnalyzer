﻿namespace DemoAnalyzer;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BadParameterAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Design";

    public const string BadParameterDiagnosticId = "BadParameter";
    private static readonly LocalizableString BadParameterTitle = new LocalizableResourceString(nameof(Resources.BadParameterAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadParameterMessageFormat = new LocalizableResourceString(nameof(Resources.BadParameterAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadParameterDescription = new LocalizableResourceString(nameof(Resources.BadParameterAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor BadParameterRule = new DiagnosticDescriptor(BadParameterDiagnosticId, BadParameterTitle, BadParameterMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: BadParameterDescription);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(BadParameterRule);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        try
        {
            Logger.Log($"BadParameter {context.GetHashCode()} {context.Compilation.GetHashCode()} {context.SemanticModel.GetHashCode()}");

            var ClassDeclaration = (ClassDeclarationSyntax)context.Node;
            AnalyzeClass(context, ClassDeclaration);
        }
        catch (Exception e)
        {
            Logger.Log(e.Message);
            Logger.Log(e.StackTrace);
        }
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
    {
        // Ignore diagnostic for classes not modeled.
        if (ClassModelManager.IsClassIgnoredForModeling(classDeclaration))
            return;

        ClassModel ClassModel = ClassModelManager.Instance.GetClassModel(context, classDeclaration);
        foreach (UnsupportedParameter Item in ClassModel.Unsupported.Parameters)
            context.ReportDiagnostic(Diagnostic.Create(BadParameterRule, Item.Location, Item.Name));
    }
}
