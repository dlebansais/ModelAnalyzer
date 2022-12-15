﻿namespace ModelAnalyzer;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BadEnsureAnalyzer : Analyzer
{
    private const string Category = "Design";
    public const string DiagnosticId = "MA0001";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.BadEnsureAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get => ImmutableArray.Create(Rule); }
    protected override string Id { get => DiagnosticId; }
    protected override SyntaxKind DiagnosticKind { get => SyntaxKind.ClassDeclaration; }

    protected override void ReportDiagnostic(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IClassModel classModel)
    {
        foreach (IUnsupportedEnsure Item in classModel.Unsupported.Ensures)
        {
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting bad ensure.");
            context.ReportDiagnostic(Diagnostic.Create(Rule, Item.Location));
        }
    }
}
