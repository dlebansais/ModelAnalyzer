namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RequireViolationAnalyzer : Analyzer
{
    public const string DiagnosticId = "MA0009";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.RequireViolationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.RequireViolationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.RequireViolationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor Rule = CreateRule(DiagnosticId, Title, MessageFormat, "Design", DiagnosticSeverity.Error, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get => ImmutableArray.Create(Rule); }
    protected override string Id { get => DiagnosticId; }
    protected override SyntaxKind DiagnosticKind { get => SyntaxKind.ClassDeclaration; }
    protected override bool IsAsyncRunRequested { get => true; }

    protected override void ReportDiagnostic(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IClassModel classModel)
    {
        if (!classModel.Unsupported.IsEmpty)
            return;

        Location Location = classDeclaration.Identifier.GetLocation();

        foreach (IRequireViolation RequireViolation in classModel.RequireViolations)
        {
            string MethodName = RequireViolation.Method.Name.Text;
            string RequireText = RequireViolation.Require.Text;

            Logger.Log(LogLevel.Error, $"Method '{MethodName}': reporting require '{RequireText}' violated.");
            context.ReportDiagnostic(Diagnostic.Create(Rule, Location, MethodName, RequireText));
        }
    }
}
