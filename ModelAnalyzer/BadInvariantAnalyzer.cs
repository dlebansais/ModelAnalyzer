namespace ModelAnalyzer;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BadInvariantAnalyzer : Analyzer
{
    private const string Category = "Design";
    public const string DiagnosticId = "MA0003";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.BadInvariantAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get => ImmutableArray.Create(Rule); }
    protected override string Id { get => DiagnosticId; }
    protected override SyntaxKind DiagnosticKind { get => SyntaxKind.CompilationUnit; }
    protected override bool IsAsyncRunRequested { get => false; }

    protected override void ReportDiagnostic(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IClassModel classModel)
    {
        foreach (IUnsupportedInvariant Item in classModel.Unsupported.Invariants)
        {
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting bad invariant.");
            context.ReportDiagnostic(Diagnostic.Create(Rule, Item.Location, Item.Text));
        }
    }
}
