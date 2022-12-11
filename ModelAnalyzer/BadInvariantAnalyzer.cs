namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using AnalysisLogger;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BadInvariantAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Design";

    public const string BadInvariantDiagnosticId = "BadInvariant";
    private static readonly LocalizableString BadInvariantTitle = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadInvariantMessageFormat = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString BadInvariantDescription = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor BadInvariantRule = new DiagnosticDescriptor(BadInvariantDiagnosticId, BadInvariantTitle, BadInvariantMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: BadInvariantDescription);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return ImmutableArray.Create(BadInvariantRule);
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.CompilationUnit);
    }

    private IAnalysisLogger Logger = Initialization.Logger;
    private ClassModelManager Manager = Initialization.Manager;

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        try
        {
            var CompilationUnit = (CompilationUnitSyntax)context.Node;

            foreach (MemberDeclarationSyntax Member in CompilationUnit.Members)
                if (Member is FileScopedNamespaceDeclarationSyntax FileScopedNamespaceDeclaration)
                    AnalyzeNamespaceMembers(context, FileScopedNamespaceDeclaration.Members);
                else if (Member is NamespaceDeclarationSyntax NamespaceDeclaration)
                    AnalyzeNamespaceMembers(context, NamespaceDeclaration.Members);
                else if (Member is ClassDeclarationSyntax ClassDeclaration)
                    AnalyzeClass(context, ClassDeclaration);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }

    private void AnalyzeNamespaceMembers(SyntaxNodeAnalysisContext context, SyntaxList<MemberDeclarationSyntax> members)
    {
        List<string> ExistingClassList = new();

        foreach (MemberDeclarationSyntax NamespaceMember in members)
            if (NamespaceMember is ClassDeclarationSyntax ClassDeclaration)
            {
                string ClassName = ClassDeclaration.Identifier.ValueText;

                if (ClassName != string.Empty)
                {
                    ExistingClassList.Add(ClassName);
                    AnalyzeClass(context, ClassDeclaration);
                }
            }

        Manager.RemoveMissingClasses(ExistingClassList);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
    {
        // Ignore diagnostic for classes not modeled.
        if (ClassModelManager.IsClassIgnoredForModeling(classDeclaration))
            return;

        CompilationContext CompilationContext = new(context);
        IClassModel ClassModel = Manager.GetClassModel(CompilationContext, classDeclaration);

        foreach (IUnsupportedInvariant Item in ClassModel.Unsupported.Invariants)
            context.ReportDiagnostic(Diagnostic.Create(BadInvariantRule, Item.Location, Item.Text));
    }
}
