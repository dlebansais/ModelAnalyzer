namespace DemoAnalyzer;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvariantViolationAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "InvariantViolation";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.InvariantViolationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.InvariantViolationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.InvariantViolationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private const string Category = "Design";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

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

        (IClassModel ClassModel, bool IsVerifyingAsynchronously) = Manager.GetClassModel(context, classDeclaration, Logger);
        
        if (IsVerifyingAsynchronously)
            ClassModel.WaitForVerifyCompleted();

        if (!ClassModel.Unsupported.IsEmpty)
            return;

        if (!Manager.IsInvariantViolated(ClassModel.Name))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.ValueText));
    }
}
