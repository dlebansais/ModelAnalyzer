namespace ModelAnalyzer;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RequireViolationAnalyzer : Analyzer
{
    public const string DiagnosticId = "MA0012";

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
        string ClassName = classDeclaration.Identifier.ValueText;
        bool ForceSynchronous = ClassName.StartsWith(InvariantViolationAnalyzer.ForSynchronousTestOnly);

        if (!classModel.Unsupported.IsEmpty)
            return;

        if (ForceSynchronous)
        {
            Logger.Log(LogLevel.Warning, "ForceSynchronous mode active");

            // TODO: use some ack frame to wait for the verifier to be ready instead.
            TimeSpan OldDelay = ClassModelManager.DelayBeforeReadingVerificationResult;
            ClassModelManager.DelayBeforeReadingVerificationResult = TimeSpan.FromSeconds(10);
            classModel = Manager.GetVerifiedModel(classModel);
            ClassModelManager.DelayBeforeReadingVerificationResult = OldDelay;
        }

        foreach (IRequireViolation RequireViolation in classModel.RequireViolations)
        {
            string MethodName = RequireViolation.Method.Name.Text;
            string RequireText = RequireViolation.Text;
            Location Location = RequireViolation.NameLocation;

            Logger.Log(LogLevel.Error, $"Method '{MethodName}': reporting require '{RequireText}' violated.");
            context.ReportDiagnostic(Diagnostic.Create(Rule, Location, MethodName, RequireText));
        }
    }
}
