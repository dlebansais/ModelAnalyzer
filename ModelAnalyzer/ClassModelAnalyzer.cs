namespace ModelAnalyzer;

using System;
using System.Collections.Immutable;
using AnalysisLogger;
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

        CompilationContext CompilationContext = CompilationContextHelper.ToCompilationContext(context);
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext, classDeclaration);

        if (ClassModel.Unsupported.IsEmpty)
            return;

        context.ReportDiagnostic(Diagnostic.Create(ClassModelRule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.ValueText));
    }
}
