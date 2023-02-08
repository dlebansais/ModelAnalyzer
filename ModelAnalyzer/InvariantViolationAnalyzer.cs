namespace ModelAnalyzer;

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
    public const string DiagnosticId = "MA0014";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.InvariantViolationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.InvariantViolationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.InvariantViolationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor Rule = CreateRule(DiagnosticId, Title, MessageFormat, "Design", DiagnosticSeverity.Error, Description);

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

        foreach (IInvariantViolation InvariantViolation in classModel.InvariantViolations)
        {
            string InvariantText = InvariantViolation.Invariant.Text;
            Location Location = InvariantViolation.Invariant.Location;

            Logger.Log(LogLevel.Error, $"Class '{ClassName}': reporting invariant '{InvariantText}' violated.");
            context.ReportDiagnostic(Diagnostic.Create(Rule, Location, ClassName, InvariantText));
        }
    }
}
