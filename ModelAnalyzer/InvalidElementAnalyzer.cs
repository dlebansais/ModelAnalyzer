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
    public const string DiagnosticIdInvalidParameter = "MA0004";
    public const string DiagnosticIdInvalidRequire = "MA0005";
    public const string DiagnosticIdInvalidStatement = "MA0006";

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

    private static readonly LocalizableString TitleInvalidParameter = new LocalizableResourceString(nameof(Resources.BadParameterAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormatInvalidParameter = new LocalizableResourceString(nameof(Resources.BadParameterAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString DescriptionInvalidParameter = new LocalizableResourceString(nameof(Resources.BadParameterAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor RuleInvalidParameter = new DiagnosticDescriptor(DiagnosticIdInvalidParameter,
                                                                                                 TitleInvalidParameter,
                                                                                                 MessageFormatInvalidParameter,
                                                                                                 Category,
                                                                                                 DiagnosticSeverity.Warning,
                                                                                                 isEnabledByDefault: true,
                                                                                                 DescriptionInvalidParameter,
                                                                                                 GetHelpLink(DiagnosticIdInvalidParameter));

    private static readonly LocalizableString TitleInvalidRequire = new LocalizableResourceString(nameof(Resources.BadRequireAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormatInvalidRequire = new LocalizableResourceString(nameof(Resources.BadRequireAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString DescriptionInvalidRequire = new LocalizableResourceString(nameof(Resources.BadRequireAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor RuleInvalidRequire = new DiagnosticDescriptor(DiagnosticIdInvalidRequire,
                                                                                               TitleInvalidRequire,
                                                                                               MessageFormatInvalidRequire,
                                                                                               Category,
                                                                                               DiagnosticSeverity.Warning,
                                                                                               isEnabledByDefault: true,
                                                                                               DescriptionInvalidRequire,
                                                                                               GetHelpLink(DiagnosticIdInvalidRequire));

    private static readonly LocalizableString TitleInvalidStatement = new LocalizableResourceString(nameof(Resources.BadStatementAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormatInvalidStatement = new LocalizableResourceString(nameof(Resources.BadStatementAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString DescriptionInvalidStatement = new LocalizableResourceString(nameof(Resources.BadStatementAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor RuleInvalidStatement = new DiagnosticDescriptor(DiagnosticIdInvalidStatement,
                                                                                                 TitleInvalidStatement,
                                                                                                 MessageFormatInvalidStatement,
                                                                                                 Category,
                                                                                                 DiagnosticSeverity.Warning,
                                                                                                 isEnabledByDefault: true,
                                                                                                 DescriptionInvalidStatement,
                                                                                                 GetHelpLink(DiagnosticIdInvalidStatement));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get => ImmutableArray.Create(RuleInvalidEnsure,
                                     RuleInvalidExpression,
                                     RuleInvalidParameter,
                                     RuleInvalidRequire,
                                     RuleInvalidStatement);
    }

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

        foreach (IUnsupportedParameter Item in ClassModel.Unsupported.Parameters)
        {
            Logger.Log(LogLevel.Warning, $"Class '{ClassModel.Name}': reporting invalid parameter.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidParameter, Item.Location, Item.Name.Text));
        }

        foreach (IUnsupportedRequire Item in ClassModel.Unsupported.Requires)
        {
            Logger.Log(LogLevel.Warning, $"Class '{ClassModel.Name}': reporting invalid require.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidRequire, Item.Location));
        }

        foreach (IUnsupportedStatement Item in ClassModel.Unsupported.Statements)
        {
            Logger.Log(LogLevel.Warning, $"Class '{ClassModel.Name}': reporting invalid statement.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidStatement, Item.Location));
        }
    }

    private static string GetHelpLink(string diagnosticId)
    {
        return $"https://github.com/dlebansais/ModelAnalyzer/blob/master/doc/{diagnosticId}.md";
    }
}
