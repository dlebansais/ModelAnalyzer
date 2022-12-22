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
    private List<Invariant> ParseInvariants(ClassDeclarationSyntax classDeclaration, FieldTable fieldTable, Unsupported unsupported)
    {
        List<Invariant> InvariantList = new();

        SyntaxToken LastToken = classDeclaration.GetLastToken();
        var Location = LastToken.GetLocation();

        if (LastToken.HasLeadingTrivia)
            ReportUnsupportedRequires(unsupported, LastToken.LeadingTrivia);

        if (LastToken.HasTrailingTrivia)
        {
            SyntaxTriviaList TrailingTrivia = LastToken.TrailingTrivia;
            AddInvariantsInTrivia(InvariantList, fieldTable, unsupported, TrailingTrivia);
            Location = TrailingTrivia.Last().GetLocation();
        }

        SyntaxNode Root = classDeclaration.SyntaxTree.GetRoot();
        int EndPosition = Location.SourceSpan.End;

        if (Root.FullSpan.Contains(EndPosition))
        {
            var NextToken = Root.FindToken(EndPosition);

            if (NextToken.HasLeadingTrivia)
                AddInvariantsInTrivia(InvariantList, fieldTable, unsupported, NextToken.LeadingTrivia);
        }

        return InvariantList;
    }

    private void AddInvariantsInTrivia(List<Invariant> invariantList, FieldTable fieldTable, Unsupported unsupported, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Header = $"// {Modeling.Invariant}";

                if (Comment.StartsWith(Header))
                    AddInvariantsInTrivia(invariantList, fieldTable, unsupported, Trivia, Comment, Header);
            }
    }

    private void AddInvariantsInTrivia(List<Invariant> invariantList, FieldTable fieldTable, Unsupported unsupported, SyntaxTrivia trivia, string comment, string header)
    {
        string Text = comment.Substring(header.Length);

        LocationContext LocationContext = new(trivia, header, AssignmentAssertionText.Length);

        if (TryParseAssertionInTrivia(fieldTable, new ParameterTable(), unsupported, Text, LocationContext, out Expression BooleanExpression, out bool IsErrorReported))
        {
            Invariant NewInvariant = new Invariant { Text = Text, BooleanExpression = BooleanExpression };

            Log($"Invariant analyzed: '{NewInvariant}'.");

            invariantList.Add(NewInvariant);
        }
        else if (!IsErrorReported)
        {
            Location Location = trivia.GetLocation();
            unsupported.AddUnsupportedInvariant(Text, Location);
        }
    }
}
