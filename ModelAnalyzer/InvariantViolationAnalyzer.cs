﻿namespace ModelAnalyzer;

using System.Collections.Immutable;
using System.Diagnostics;
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

    protected override void ReportDiagnostic(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IClassModel classModel)
    {
        Logger.Log("Starting the process.");
        Process.Start(@"C:\Projects\Temp\ModelAnalyzer\Verifier\bin\x64\Debug\net48\Verifier.exe");
        Logger.Log("Started.");

        if (!classModel.Unsupported.IsEmpty)
            return;

        if (!classModel.IsInvariantViolated)
            return;

        Location Location = classDeclaration.Identifier.GetLocation();

        Logger.Log(LogLevel.Error, $"Class '{classModel.Name}': reporting invariant violated.");
        context.ReportDiagnostic(Diagnostic.Create(Rule, Location, classDeclaration.Identifier.ValueText));
    }
}
