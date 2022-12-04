namespace DemoAnalyzer;

using System;
using System.Collections.Immutable;
using AnalysisLogger;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BadEnsureAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Design";

    public const string BadEnsureDiagnosticId = "BadEnsure";
    private static readonly LocalizableString BadEnsureTitle = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadEnsureMessageFormat = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadEnsureDescription = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor BadEnsureRule = new DiagnosticDescriptor(BadEnsureDiagnosticId, BadEnsureTitle, BadEnsureMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: BadEnsureDescription);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(BadEnsureRule);
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

        IClassModel ClassModel = Manager.GetClassModel(context, classDeclaration);

        foreach (IUnsupportedEnsure Item in ClassModel.Unsupported.Ensures)
            context.ReportDiagnostic(Diagnostic.Create(BadEnsureRule, Item.Location));
    }
}
