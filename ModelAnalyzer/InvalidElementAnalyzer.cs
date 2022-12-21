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
    public const string DiagnosticId0 = "MA0001";
    public const string DiagnosticId1 = "MA0002";

    private static readonly LocalizableString Title0 = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat0 = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description0 = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor Rule0 = new DiagnosticDescriptor(DiagnosticId0, Title0, MessageFormat0, "Design", DiagnosticSeverity.Warning, isEnabledByDefault: true, Description0, $"https://github.com/dlebansais/ModelAnalyzer/blob/master/doc/{DiagnosticId0}.md");

    private static readonly LocalizableString Title1 = new LocalizableResourceString(nameof(Resources.BadExpressionAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat1 = new LocalizableResourceString(nameof(Resources.BadExpressionAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description1 = new LocalizableResourceString(nameof(Resources.BadExpressionAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor Rule1 = new DiagnosticDescriptor(DiagnosticId1, Title1, MessageFormat1, "Design", DiagnosticSeverity.Warning, isEnabledByDefault: true, Description1, $"https://github.com/dlebansais/ModelAnalyzer/blob/master/doc/{DiagnosticId1}.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get => ImmutableArray.Create(Rule0, Rule1); }

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
            Logger.Log(LogLevel.Warning, $"Class '{ClassModel.Name}': reporting bad ensure.");
            context.ReportDiagnostic(Diagnostic.Create(Rule0, Item.Location));
        }

        foreach (IUnsupportedExpression Item in ClassModel.Unsupported.Expressions)
        {
            Logger.Log(LogLevel.Warning, $"Class '{ClassModel.Name}': reporting bad expression.");
            context.ReportDiagnostic(Diagnostic.Create(Rule1, Item.Location));
        }
    }
}
