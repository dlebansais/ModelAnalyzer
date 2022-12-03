namespace DemoAnalyzer;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BadExpressionAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Design";

    public const string BadExpressionDiagnosticId = "BadExpression";
    private static readonly LocalizableString BadExpressionTitle = new LocalizableResourceString(nameof(Resources.BadExpressionAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadExpressionMessageFormat = new LocalizableResourceString(nameof(Resources.BadExpressionAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadExpressionDescription = new LocalizableResourceString(nameof(Resources.BadExpressionAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor BadExpressionRule = new DiagnosticDescriptor(BadExpressionDiagnosticId, BadExpressionTitle, BadExpressionMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: BadExpressionDescription);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(BadExpressionRule);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private ILogger Logger = Initialization.Logger;
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

        (IClassModel ClassModel, _) = Manager.GetClassModel(context, classDeclaration, Logger);
        foreach (IUnsupportedExpression Item in ClassModel.Unsupported.Expressions)
            context.ReportDiagnostic(Diagnostic.Create(BadExpressionRule, Item.Location));
    }
}
