namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
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
    public const string DiagnosticIdInvalidField = "MA0002";
    public const string DiagnosticIdInvalidMethod = "MA0003";
    public const string DiagnosticIdInvalidParameter = "MA0004";
    public const string DiagnosticIdInvalidRequire = "MA0005";
    public const string DiagnosticIdInvalidEnsure = "MA0006";
    public const string DiagnosticIdInvalidLocal = "MA0007";
    public const string DiagnosticIdInvalidStatement = "MA0008";
    public const string DiagnosticIdInvalidExpression = "MA0009";
    public const string DiagnosticIdInvalidProperty = "MA0010";
    public const string DiagnosticIdInvalidInvariant = "MA0011";

    private static readonly LocalizableString TitleInvalidProperty = new LocalizableResourceString(nameof(Resources.BadPropertyAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormatInvalidProperty = new LocalizableResourceString(nameof(Resources.BadPropertyAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString DescriptionInvalidProperty = new LocalizableResourceString(nameof(Resources.BadPropertyAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor RuleInvalidProperty = new DiagnosticDescriptor(DiagnosticIdInvalidProperty,
                                                                                                TitleInvalidProperty,
                                                                                                MessageFormatInvalidProperty,
                                                                                                Category,
                                                                                                DiagnosticSeverity.Warning,
                                                                                                isEnabledByDefault: true,
                                                                                                DescriptionInvalidProperty,
                                                                                                GetHelpLink(DiagnosticIdInvalidProperty));

    private static readonly LocalizableString TitleInvalidField = new LocalizableResourceString(nameof(Resources.BadFieldAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormatInvalidField = new LocalizableResourceString(nameof(Resources.BadFieldAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString DescriptionInvalidField = new LocalizableResourceString(nameof(Resources.BadFieldAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor RuleInvalidField = new DiagnosticDescriptor(DiagnosticIdInvalidField,
                                                                                             TitleInvalidField,
                                                                                             MessageFormatInvalidField,
                                                                                             Category,
                                                                                             DiagnosticSeverity.Warning,
                                                                                             isEnabledByDefault: true,
                                                                                             DescriptionInvalidField,
                                                                                             GetHelpLink(DiagnosticIdInvalidField));

    private static readonly LocalizableString TitleInvalidMethod = new LocalizableResourceString(nameof(Resources.BadMethodAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormatInvalidMethod = new LocalizableResourceString(nameof(Resources.BadMethodAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString DescriptionInvalidMethod = new LocalizableResourceString(nameof(Resources.BadMethodAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor RuleInvalidMethod = new DiagnosticDescriptor(DiagnosticIdInvalidMethod,
                                                                                              TitleInvalidMethod,
                                                                                              MessageFormatInvalidMethod,
                                                                                              Category,
                                                                                              DiagnosticSeverity.Warning,
                                                                                              isEnabledByDefault: true,
                                                                                              DescriptionInvalidMethod,
                                                                                              GetHelpLink(DiagnosticIdInvalidMethod));

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

    private static readonly LocalizableString TitleInvalidLocal = new LocalizableResourceString(nameof(Resources.BadLocalAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormatInvalidLocal = new LocalizableResourceString(nameof(Resources.BadLocalAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString DescriptionInvalidLocal = new LocalizableResourceString(nameof(Resources.BadLocalAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor RuleInvalidLocal = new DiagnosticDescriptor(DiagnosticIdInvalidLocal,
                                                                                             TitleInvalidLocal,
                                                                                             MessageFormatInvalidLocal,
                                                                                             Category,
                                                                                             DiagnosticSeverity.Warning,
                                                                                             isEnabledByDefault: true,
                                                                                             DescriptionInvalidLocal,
                                                                                             GetHelpLink(DiagnosticIdInvalidLocal));

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

    private static readonly LocalizableString TitleInvalidInvariant = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormatInvalidInvariant = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString DescriptionInvalidInvariant = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor RuleInvalidInvariant = new DiagnosticDescriptor(DiagnosticIdInvalidInvariant,
                                                                                                 TitleInvalidInvariant,
                                                                                                 MessageFormatInvalidInvariant,
                                                                                                 Category,
                                                                                                 DiagnosticSeverity.Warning,
                                                                                                 isEnabledByDefault: true,
                                                                                                 DescriptionInvalidInvariant,
                                                                                                 GetHelpLink(DiagnosticIdInvalidInvariant));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get => ImmutableArray.Create(RuleInvalidProperty,
                                     RuleInvalidField,
                                     RuleInvalidMethod,
                                     RuleInvalidParameter,
                                     RuleInvalidRequire,
                                     RuleInvalidEnsure,
                                     RuleInvalidLocal,
                                     RuleInvalidStatement,
                                     RuleInvalidExpression,
                                     RuleInvalidInvariant);
    }

    private IAnalysisLogger Logger { get; } = Initialization.Logger;
    private ClassModelManager Manager { get; } = Initialization.Manager;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeCompilationUnit, SyntaxKind.CompilationUnit);
    }

    private void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
    {
        try
        {
            CompilationUnitSyntax CompilationUnit = (CompilationUnitSyntax)context.Node;
            List<string> ExistingClassList = new();
            List<ClassDeclarationSyntax> ClassDeclarationList = new();

            foreach (MemberDeclarationSyntax Member in CompilationUnit.Members)
                if (Member is FileScopedNamespaceDeclarationSyntax FileScopedNamespaceDeclaration)
                    AnalyzeNamespaceMembers(context, FileScopedNamespaceDeclaration.Members, ExistingClassList, ClassDeclarationList);
                else if (Member is NamespaceDeclarationSyntax NamespaceDeclaration)
                    AnalyzeNamespaceMembers(context, NamespaceDeclaration.Members, ExistingClassList, ClassDeclarationList);
                else if (Member is ClassDeclarationSyntax ClassDeclaration)
                    AddClassDeclaration(ExistingClassList, ClassDeclarationList, ClassDeclaration);

            AnalyzeClasses(context, CompilationUnit, ClassDeclarationList);

            Manager.RemoveMissingClasses(ExistingClassList);
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

    private void AnalyzeNamespaceMembers(SyntaxNodeAnalysisContext context, SyntaxList<MemberDeclarationSyntax> members, List<string> existingClassList, List<ClassDeclarationSyntax> classDeclarationList)
    {
        foreach (MemberDeclarationSyntax NamespaceMember in members)
            if (NamespaceMember is ClassDeclarationSyntax ClassDeclaration)
                AddClassDeclaration(existingClassList, classDeclarationList, ClassDeclaration);
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
        foreach (IUnsupportedProperty Item in classModel.Unsupported.Properties)
        {
            string PropertyName = Tools.Truncate(Item.Name.Text);
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting invalid property '{PropertyName}'.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidProperty, Item.Location, PropertyName));
        }

        foreach (IUnsupportedField Item in classModel.Unsupported.Fields)
        {
            string FieldName = Tools.Truncate(Item.Name.Text);
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting invalid field '{FieldName}'.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidField, Item.Location, FieldName));
        }

        foreach (IUnsupportedMethod Item in classModel.Unsupported.Methods)
        {
            string MethodName = Tools.Truncate(Item.Name.Text);
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting invalid method '{MethodName}'.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidMethod, Item.Location, MethodName));
        }

        foreach (IUnsupportedParameter Item in classModel.Unsupported.Parameters)
        {
            string ParameterName = Tools.Truncate(Item.Name.Text);
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting invalid parameter '{ParameterName}'.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidParameter, Item.Location, ParameterName));
        }

        foreach (IUnsupportedRequire Item in classModel.Unsupported.Requires)
        {
            string AssertionText = Tools.Truncate(Item.Text);
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting invalid require.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidRequire, Item.Location, AssertionText));
        }

        foreach (IUnsupportedEnsure Item in classModel.Unsupported.Ensures)
        {
            string AssertionText = Tools.Truncate(Item.Text);
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting invalid ensure.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidEnsure, Item.Location, AssertionText));
        }

        foreach (IUnsupportedLocal Item in classModel.Unsupported.Locals)
        {
            string LocalName = Tools.Truncate(Item.Name.Text);
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting invalid local variable '{LocalName}'.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidLocal, Item.Location, LocalName));
        }

        foreach (IUnsupportedStatement Item in classModel.Unsupported.Statements)
        {
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting invalid statement.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidStatement, Item.Location));
        }

        foreach (IUnsupportedExpression Item in classModel.Unsupported.Expressions)
        {
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting invalid expression.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidExpression, Item.Location));
        }

        foreach (IUnsupportedInvariant Item in classModel.Unsupported.Invariants)
        {
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting bad invariant.");
            context.ReportDiagnostic(Diagnostic.Create(RuleInvalidInvariant, Item.Location, Item.Text));
        }
    }

    private static string GetHelpLink(string diagnosticId)
    {
        return $"https://github.com/dlebansais/ModelAnalyzer/blob/master/doc/{diagnosticId}.md";
    }
}
