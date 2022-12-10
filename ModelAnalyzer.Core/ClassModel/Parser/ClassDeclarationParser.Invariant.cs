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
    private List<IInvariant> ParseInvariants(ClassDeclarationSyntax classDeclaration, FieldTable fieldTable, Unsupported unsupported)
    {
        List<IInvariant> InvariantList = new();

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

    private void AddInvariantsInTrivia(List<IInvariant> invariantList, FieldTable fieldTable, Unsupported unsupported, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Invariant}";

                if (Comment.StartsWith(Pattern))
                    AddInvariantsInTrivia(invariantList, fieldTable, unsupported, Trivia, Comment, Pattern);
            }
    }

    private void AddInvariantsInTrivia(List<IInvariant> invariantList, FieldTable fieldTable, Unsupported unsupported, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);
        IInvariant NewInvariant;

        if (TryParseAssertionInTrivia(fieldTable, new ParameterTable(), unsupported, Text, out IExpression BooleanExpression))
        {
            NewInvariant = new Invariant { Text = Text, BooleanExpression = BooleanExpression };

            Log($"Invariant analyzed: '{NewInvariant}'.");
        }
        else
        {
            Location Location = GetLocationInComment(trivia, pattern);
            unsupported.AddUnsupportedInvariant(Text, Location, out IUnsupportedInvariant UnsupportedInvariant);

            NewInvariant = UnsupportedInvariant;
        }

        invariantList.Add(NewInvariant);
    }
}
