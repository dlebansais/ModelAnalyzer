namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ClassModelAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Design";

    public const string ClassModelDiagnosticId = "ClassModel";
    private static readonly LocalizableString ClassModelTitle = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString ClassModelMessageFormat = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString ClassModelDescription = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor ClassModelRule = new DiagnosticDescriptor(ClassModelDiagnosticId, ClassModelTitle, ClassModelMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: ClassModelDescription);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(ClassModelRule);
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
        // Ignore diagnostic for classes not modeled.
        if (ClassModel.IsClassIgnoredForModeling(classDeclaration))
            return;

        ClassModel NewClassModel = ClassModel.FromClassDeclaration(classDeclaration);
        if (NewClassModel.IsSupported)
        {
            ClassModelManager.Instance.Update(NewClassModel);
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(ClassModelRule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.ValueText));
    }
}
