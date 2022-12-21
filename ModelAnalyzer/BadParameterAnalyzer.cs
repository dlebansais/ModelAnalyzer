namespace ModelAnalyzer;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BadParameterAnalyzer : Analyzer
{
    public const string DiagnosticId = "MA0004";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.BadParameterAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.BadParameterAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.BadParameterAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor Rule = CreateRule(DiagnosticId, Title, MessageFormat, "Design", DiagnosticSeverity.Warning, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get => ImmutableArray.Create(Rule); }
    protected override string Id { get => DiagnosticId; }
    protected override SyntaxKind DiagnosticKind { get => SyntaxKind.ClassDeclaration; }
    protected override bool IsAsyncRunRequested { get => false; }

    protected override void ReportDiagnostic(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IClassModel classModel)
    {
        foreach (IUnsupportedParameter Item in classModel.Unsupported.Parameters)
        {
            Logger.Log(LogLevel.Warning, $"Class '{classModel.Name}': reporting bad parameter.");
            context.ReportDiagnostic(Diagnostic.Create(Rule, Item.Location, Item.Name));
        }
    }
}
