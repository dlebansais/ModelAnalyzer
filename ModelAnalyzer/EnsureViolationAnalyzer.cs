namespace ModelAnalyzer;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnsureViolationAnalyzer : Analyzer
{
    public const string DiagnosticId = "MA0012";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.EnsureViolationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.EnsureViolationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.EnsureViolationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
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
            classModel = Manager.GetVerifiedModel(classModel);
        }

        foreach (IEnsureViolation EnsureViolation in classModel.EnsureViolations)
        {
            string MethodName = EnsureViolation.Method.Name.Text;
            string EnsureText = EnsureViolation.Ensure.Text;
            Location Location = EnsureViolation.Ensure.Location;

            Logger.Log(LogLevel.Error, $"Method '{MethodName}': reporting require '{EnsureText}' violated.");
            context.ReportDiagnostic(Diagnostic.Create(Rule, Location, MethodName, EnsureText));
        }
    }
}
