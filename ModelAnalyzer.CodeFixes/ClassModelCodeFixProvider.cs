namespace ModelAnalyzer;

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClassModelCodeFixProvider)), Shared]
public class ClassModelCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
    {
        get { return ImmutableArray.Create(ClassModelAnalyzer.DiagnosticId); }
    }

    public sealed override FixAllProvider GetFixAllProvider()
    {
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the type declaration identified by the diagnostic.
        var declaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

        if (declaration is not null)
        {
            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.ClassModelCodeFixTitle,
                    createChangedDocument: c => ClassModelAsync(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.ClassModelCodeFixTitle)),
                diagnostic);
        }
    }

    private static async Task<Document> ClassModelAsync(Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        // Add a comment to the leading trivia.
        SyntaxToken firstToken = classDeclaration.GetFirstToken();
        SyntaxTriviaList leadingTrivia = firstToken.LeadingTrivia;

        string NewLine = "\n";
        foreach (SyntaxTrivia Trivia in leadingTrivia)
            if (Trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                NewLine = Trivia.ToFullString();
                break;
            }

        SyntaxTrivia commentTrivia = SyntaxFactory.Comment($"// {Modeling.None}");
        SyntaxTrivia endOfLineTrivia = SyntaxFactory.EndOfLine(NewLine);
        SyntaxTriviaList modifiedTrivia = leadingTrivia.Add(commentTrivia).Add(endOfLineTrivia);

        ClassDeclarationSyntax newClassDeclaration = classDeclaration.ReplaceToken(
            firstToken, firstToken.WithLeadingTrivia(modifiedTrivia));

        // Add an annotation to format the new local declaration.
        ClassDeclarationSyntax formattedClassDeclaration = newClassDeclaration.WithAdditionalAnnotations(Formatter.Annotation);

        // Replace the old class declaration with the new class declaration.
        SyntaxNode? oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        SyntaxNode? newRoot = oldRoot?.ReplaceNode(classDeclaration, formattedClassDeclaration);

        // Return document with transformed tree.
        return newRoot is not null ? document.WithSyntaxRoot(newRoot) : document;
    }
}
