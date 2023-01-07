namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AnalysisLogger;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ClassModelAnalyzer : DiagnosticAnalyzer
{
    protected IAnalysisLogger Logger { get; } = Initialization.Logger;
    protected ClassModelManager Manager { get; } = Initialization.Manager;

    public const string DiagnosticId = "MA0001";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Design", DiagnosticSeverity.Warning, isEnabledByDefault: true, Description, Tools.GetHelpLink(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get => ImmutableArray.Create(Rule); }

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
            List<string> ExistingClassList = new();
            List<ClassDeclarationSyntax> ClassDeclarationList = new();

            ClassDeclarationSyntax ClassDeclaration = (ClassDeclarationSyntax)context.Node;
            AddClassDeclaration(ExistingClassList, ClassDeclarationList, ClassDeclaration);

            CompilationUnitSyntax CompilationUnit = ClassDeclaration.AncestorsAndSelf().OfType<CompilationUnitSyntax>().First();
            AnalyzeClasses(context, CompilationUnit, ClassDeclarationList);
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

    private void AddClassDeclaration(List<string> existingClassList, List<ClassDeclarationSyntax> classDeclarationList, ClassDeclarationSyntax classDeclaration)
    {
        existingClassList.Add(classDeclaration.Identifier.ValueText);

        // Ignore diagnostic for classes not modeled.
        if (!ClassModelManager.IsClassIgnoredForModeling(classDeclaration))
            classDeclarationList.Add(classDeclaration);
    }

    private void AnalyzeClasses(SyntaxNodeAnalysisContext context, CompilationUnitSyntax compilationUnit, List<ClassDeclarationSyntax> classDeclarationList)
    {
        CompilationContext CompilationContext = CompilationContextHelper.ToCompilationContext(compilationUnit, isAsyncRunRequested: false);
        AnalyzerSemanticModel SemanticModel = new(context.SemanticModel);
        IDictionary<ClassDeclarationSyntax, IClassModel> ClassModelTable = Manager.GetClassModels(CompilationContext, classDeclarationList, SemanticModel);

        foreach (KeyValuePair<ClassDeclarationSyntax, IClassModel> Entry in ClassModelTable)
            ReportDiagnostic(context, Entry.Key, Entry.Value);
    }

    protected void ReportDiagnostic(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IClassModel classModel)
    {
        if (classModel.Unsupported.IsEmpty)
            return;

        Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting unsupported elements.");
        context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.ValueText));
    }
}
