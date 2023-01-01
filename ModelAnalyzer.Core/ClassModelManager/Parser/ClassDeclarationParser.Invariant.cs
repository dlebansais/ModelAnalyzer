namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private List<Invariant> ParseInvariants(ParsingContext parsingContext, ClassDeclarationSyntax classDeclaration)
    {
        List<Invariant> InvariantList = new();

        SyntaxToken LastToken = classDeclaration.GetLastToken();
        var Location = LastToken.GetLocation();

        if (LastToken.HasLeadingTrivia)
            ReportUnsupportedRequires(parsingContext, LastToken.LeadingTrivia);

        if (LastToken.HasTrailingTrivia)
        {
            SyntaxTriviaList TrailingTrivia = LastToken.TrailingTrivia;
            AddInvariantsInTrivia(parsingContext, InvariantList, TrailingTrivia);
            Location = TrailingTrivia.Last().GetLocation();
        }

        SyntaxNode Root = classDeclaration.SyntaxTree.GetRoot();
        int EndPosition = Location.SourceSpan.End;

        if (Root.FullSpan.Contains(EndPosition))
        {
            var NextToken = Root.FindToken(EndPosition);

            if (NextToken.HasLeadingTrivia)
                AddInvariantsInTrivia(parsingContext, InvariantList, NextToken.LeadingTrivia);
        }

        return InvariantList;
    }

    private void AddInvariantsInTrivia(ParsingContext parsingContext, List<Invariant> invariantList, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Header = $"// {Modeling.Invariant}";

                if (Comment.StartsWith(Header))
                    AddInvariantsInTrivia(parsingContext, invariantList, Trivia, Comment, Header);
            }
    }

    private void AddInvariantsInTrivia(ParsingContext parsingContext, List<Invariant> invariantList, SyntaxTrivia trivia, string comment, string header)
    {
        string Text = comment.Substring(header.Length);

        Log($"Analyzing ensure '{Text}'.");

        Invariant? NewInvariant = null;
        bool IsErrorReported = false;

        if (TryParseAssertionTextInTrivia(Text, out SyntaxTree SyntaxTree, out int Offset))
        {
            LocationContext LocationContext = new(trivia, header, Offset);

            if (IsValidAssertionSyntaxTree(parsingContext, isLocalAllowed: false, resultLocal: null, LocationContext, SyntaxTree, out Expression BooleanExpression, out IsErrorReported))
            {
                NewInvariant = new Invariant { Text = Text, Location = trivia.GetLocation(), BooleanExpression = BooleanExpression };
            }
        }

        if (NewInvariant is not null)
        {
            Log($"Invariant analyzed: '{NewInvariant}'.");

            invariantList.Add(NewInvariant);
        }
        else if (!IsErrorReported)
        {
            Location Location = trivia.GetLocation();
            parsingContext.Unsupported.AddUnsupportedInvariant(Text, Location);
        }
    }
}
