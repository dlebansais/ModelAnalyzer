namespace ModelAnalyzer;

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AnalysisLogger;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvariantViolationAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MA0008";

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

        /*
        Location Location = classDeclaration.Identifier.GetLocation();
        string ClassName = classDeclaration.Identifier.ValueText;

        CompilationContext CompilationContext = CompilationContextHelper.ToCompilationContext(DiagnosticId, classDeclaration, isAsyncRunRequested: true);
        Task<IClassModel> GetClassModelTask = Manager.GetClassModelAsync(CompilationContext, classDeclaration);

        // Don't wait too long and get the analyzer stuck.
        GetClassModelTask.Wait(TimeSpan.FromSeconds(5));

        if (!GetClassModelTask.IsCompleted)
        {
            Logger.Log(LogLevel.Warning, $"Timeout waiting for analysis of class '{ClassName}' to complete.");
            return;
        }

        IClassModel ClassModel = GetClassModelTask.Result;

        if (!ClassModel.Unsupported.IsEmpty)
            return;

        if (!ClassModel.IsInvariantViolated)
            return;

        Logger.Log(LogLevel.Error, $"Class '{ClassName}': reporting invariant violated.");
        context.ReportDiagnostic(Diagnostic.Create(Rule, Location, ClassName));
        */
    }
}
