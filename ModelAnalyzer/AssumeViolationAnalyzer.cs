namespace ModelAnalyzer;

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssumeViolationAnalyzer : Analyzer
{
    public const string DiagnosticId = "MA0015";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AssumeViolationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AssumeViolationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AssumeViolationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor Rule = CreateRule(DiagnosticId, Title, MessageFormat, "Design", DiagnosticSeverity.Error, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get => ImmutableArray.Create(Rule); }
    protected override string Id { get => DiagnosticId; }
    protected override SyntaxKind DiagnosticKind { get => SyntaxKind.ClassDeclaration; }
    protected override bool IsAsyncRunRequested { get => true; }

    protected override void BeforeInitialize()
    {
        Manager.WaitReady();
    }

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

        foreach (IAssumeViolation AssumeViolation in classModel.AssumeViolations)
        {
            string AssumeText = AssumeViolation.Text;
            Location Location = AssumeViolation.Location;

            if (AssumeViolation.Method is IMethod Method)
            {
                string MethodName = Method.Name.Text;
                Logger.Log(LogLevel.Error, $"Method '{MethodName}': reporting flow check '{AssumeText}' violated.");
            }
            else
                Logger.Log(LogLevel.Error, $"Reporting flow check '{AssumeText}' violated.");

            context.ReportDiagnostic(Diagnostic.Create(Rule, Location, AssumeText));
        }
    }
}
