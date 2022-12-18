﻿namespace ModelAnalyzer;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvariantViolationAnalyzer : Analyzer
{
    private const string Category = "Design";
    public const string DiagnosticId = "MA0008";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.InvariantViolationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.InvariantViolationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.InvariantViolationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get => ImmutableArray.Create(Rule); }
    protected override string Id { get => DiagnosticId; }
    protected override SyntaxKind DiagnosticKind { get => SyntaxKind.ClassDeclaration; }
    protected override bool IsAsyncRunRequested { get => true; }

    public const string ForSynchronousTestOnly = "ClassName_62D72B24_5F04_451F_BC32_ABE6D787701B";

    protected override void ReportDiagnostic(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IClassModel classModel)
    {
        string ClassName = classDeclaration.Identifier.ValueText;
        bool ForceSynchronous = ClassName.StartsWith(ForSynchronousTestOnly);

        if (!classModel.Unsupported.IsEmpty)
            return;

        if (ForceSynchronous)
        {
            Logger.Log(LogLevel.Warning, "ForceSynchronous mode active");
            classModel = Manager.GetVerifiedModel(classModel);
        }

        if (!classModel.IsInvariantViolated)
            return;

        Location Location = classDeclaration.Identifier.GetLocation();

        Logger.Log(LogLevel.Error, $"Class '{classModel.Name}': reporting invariant violated.");
        context.ReportDiagnostic(Diagnostic.Create(Rule, Location, classDeclaration.Identifier.ValueText));
    }
}
