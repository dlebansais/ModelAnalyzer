namespace DemoAnalyzer;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ClassModelAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ClassModel";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private const string Category = "Design";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // Check whether the class can be modeled
        SyntaxToken firstToken = classDeclaration.GetFirstToken();
        SyntaxTriviaList leadingTrivia = firstToken.LeadingTrivia;
        foreach (SyntaxTrivia Trivia in leadingTrivia)
            if (Trivia.Kind() == SyntaxKind.SingleLineCommentTrivia)
            {
                string Comment = Trivia.ToFullString();
                if (Comment.StartsWith($"// {Modeling.None}"))
                    return;
            }

        List<FieldDeclarationSyntax> Fields = new();
        bool HasUnsupported = false;
        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
        {
            switch (Member)
            {
                case FieldDeclarationSyntax AsField:
                    Fields.Add(AsField);
                    break;
                default:
                    HasUnsupported = true;
                    break;
            }
        }

        if (HasUnsupported)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.ValueText));
        }
    }
}
