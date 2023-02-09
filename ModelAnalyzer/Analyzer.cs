namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnalysisLogger;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

public abstract class Analyzer : DiagnosticAnalyzer
{
    protected IAnalysisLogger Logger { get; } = Initialization.Logger;
    protected ClassModelManager Manager { get; } = Initialization.Manager;

    protected abstract string Id { get; }
    protected abstract SyntaxKind DiagnosticKind { get; }
    protected abstract bool IsAsyncRunRequested { get; }

    protected static DiagnosticDescriptor CreateRule(string id, LocalizableString title, LocalizableString messageFormat, string category, DiagnosticSeverity diagnosticSeverity, LocalizableString description)
    {
        return new DiagnosticDescriptor(id, title, messageFormat, category, diagnosticSeverity, isEnabledByDefault: true, description, $"https://github.com/dlebansais/ModelAnalyzer/blob/master/doc/{id}.md");
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, DiagnosticKind);
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
        CompilationContext CompilationContext = CompilationContextHelper.ToCompilationContext(compilationUnit, isAsyncRunRequested: IsAsyncRunRequested);
        AnalyzerSemanticModel SemanticModel = new(context.SemanticModel);
        IDictionary<ClassDeclarationSyntax, IClassModel> ClassModelTable = Manager.GetClassModels(CompilationContext, classDeclarationList, SemanticModel);

        foreach (KeyValuePair<ClassDeclarationSyntax, IClassModel> Entry in ClassModelTable)
            ReportDiagnostic(context, Entry.Key, Entry.Value);
    }

    protected abstract void ReportDiagnostic(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IClassModel classModel);
}
