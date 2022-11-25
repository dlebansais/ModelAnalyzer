namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BadInvariantAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Design";

    public const string BadInvariantDiagnosticId = "BadInvariant";
    private static readonly LocalizableString BadInvariantTitle = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadInvariantMessageFormat = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadInvariantDescription = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor BadInvariantRule = new DiagnosticDescriptor(BadInvariantDiagnosticId, BadInvariantTitle, BadInvariantMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: BadInvariantDescription);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(BadInvariantRule);
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
        // Check whether the class can be modeled
        if (ClassModel.IsClassIgnoredForModeling(classDeclaration))
            return;

        ClassModel NewClassModel = ClassModel.FromClassDeclaration(classDeclaration);
        if (NewClassModel.IsSupported)
            return;

        foreach (IInvariant Item in NewClassModel.InvariantList)
            if (Item is UnsupportedInvariant Unsupported)
                context.ReportDiagnostic(Diagnostic.Create(BadInvariantRule, Unsupported.Location, Item.Text));
    }
}
