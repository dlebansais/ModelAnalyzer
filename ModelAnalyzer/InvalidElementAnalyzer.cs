namespace ModelAnalyzer;

using System;
using System.Collections.Immutable;
using AnalysisLogger;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvalidElementAnalyzer : DiagnosticAnalyzer
{
    public const string Category = "Design";
    public const string DiagnosticIdInvalidEnsure = "MA0001";
    public const string DiagnosticIdInvalidExpression = "MA0002";

    private static readonly LocalizableString TitleInvalidEnsure = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormatInvalidEnsure = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString DescriptionInvalidEnsure = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor RuleInvalidEnsure = new DiagnosticDescriptor(DiagnosticIdInvalidEnsure,
                                                                                              TitleInvalidEnsure,
                                                                                              MessageFormatInvalidEnsure,
                                                                                              Category,
                                                                                              DiagnosticSeverity.Warning,
                                                                                              isEnabledByDefault: true,
                                                                                              DescriptionInvalidEnsure,
                                                                                              GetHelpLink(DiagnosticIdInvalidEnsure));

    private static readonly LocalizableString TitleInvalidExpression = new LocalizableResourceString(nameof(Resources.BadExpressionAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormatInvalidExpression = new LocalizableResourceString(nameof(Resources.BadExpressionAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString DescriptionInvalidExpression = new LocalizableResourceString(nameof(Resources.BadExpressionAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor RuleInvalidExpression = new DiagnosticDescriptor(DiagnosticIdInvalidExpression,
                                                                                                  TitleInvalidExpression,
                                                                                                  MessageFormatInvalidExpression,
                                                                                                  Category,
                                                                                                  DiagnosticSeverity.Warning,
                                                                                                  isEnabledByDefault: true,
                                                                                                  DescriptionInvalidExpression,
                                                                                                  GetHelpLink(DiagnosticIdInvalidExpression));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get => ImmutableArray.Create(RuleInvalidEnsure, RuleInvalidExpression); }

    private IAnalysisLogger Logger { get; } = Initialization.Logger;
    private ClassModelManager Manager { get; } = Initialization.Manager;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
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
        catch (Exception exception)
        {
            Logger.LogException(exception);
        }
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
    {
        // Ignore diagnostic for classes not modeled.
        if (ClassModelManager.IsClassIgnoredForModeling(classDeclaration))
            return;

        CompilationContext CompilationContext = CompilationContextHelper.ToCompilationContext(classDeclaration, isAsyncRunRequested: false);
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext, classDeclaration);

        foreach (IUnsupportedEnsure Item in ClassModel.Unsupported.Ensures)
        {
            Logger.Log(LogLevel.Warning, $"Class '{ClassModel.Name}': reporting invalid ensure.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidEnsure, Item.Location));
        }

        foreach (IUnsupportedExpression Item in ClassModel.Unsupported.Expressions)
        {
            Logger.Log(LogLevel.Warning, $"Class '{ClassModel.Name}': reporting invalid expression.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidExpression, Item.Location));
        }
    }

    private static string GetHelpLink(string diagnosticId)
    {
        return $"https://github.com/dlebansais/ModelAnalyzer/blob/master/doc/{diagnosticId}.md";
    }
}
