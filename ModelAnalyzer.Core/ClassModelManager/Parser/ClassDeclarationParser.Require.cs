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
    private List<Require> ParseRequires(ParsingContext parsingContext, MethodDeclarationSyntax methodDeclaration)
    {
        List<Require> RequireList;

        if (methodDeclaration.Body is BlockSyntax Block && Block.HasLeadingTrivia)
            RequireList = ParseRequires(parsingContext, Block.GetLeadingTrivia());
        else
            RequireList = new();

        return RequireList;
    }

    private List<Require> ParseRequires(ParsingContext parsingContext, SyntaxTriviaList triviaList)
    {
        List<Require> RequireList = new();

        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string RequireHeader = $"// {Modeling.Require}";
                string EnsureHeader = $"// {Modeling.Ensure}";

                if (Comment.StartsWith(RequireHeader))
                    ParseRequire(parsingContext, RequireList, Trivia, Comment, RequireHeader);
                else if (Comment.StartsWith(EnsureHeader))
                    ReportUnsupportedEnsure(parsingContext, Trivia, Comment, EnsureHeader);
            }

        return RequireList;
    }

    private void ParseRequire(ParsingContext parsingContext, List<Require> requireList, SyntaxTrivia trivia, string comment, string header)
    {
        string Text = comment.Substring(header.Length);

        Log($"Analyzing require '{Text}'.");

        Require? NewRequire = null;
        bool IsErrorReported = false;

        if (TryParseAssertionTextInTrivia(Text, out SyntaxTree SyntaxTree, out int Offset))
        {
            LocationContext LocationContext = new(trivia, header, Offset);
            ParsingContext EnsureParsingContext = parsingContext with { IsLocalAllowed = false, ResultLocal = null, LocationContext = LocationContext, IsExpressionNested = false };

            if (IsValidAssertionSyntaxTree(EnsureParsingContext, SyntaxTree, out Expression BooleanExpression, out IsErrorReported))
            {
                NewRequire = new Require { Text = Text, Location = trivia.GetLocation(), BooleanExpression = BooleanExpression };
            }
        }

        if (NewRequire is not null)
        {
            Log($"Require analyzed: '{NewRequire}'.");

            requireList.Add(NewRequire);
        }
        else if (!IsErrorReported)
        {
            Location Location = trivia.GetLocation();
            parsingContext.Unsupported.AddUnsupportedRequire(Text, Location);
        }
    }

    private void ReportUnsupportedRequires(ParsingContext parsingContext, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Header = $"// {Modeling.Require}";

                if (Comment.StartsWith(Header))
                    ReportUnsupportedRequire(parsingContext, Trivia, Comment, Header);
            }
    }

    private void ReportUnsupportedRequire(ParsingContext parsingContext, SyntaxTrivia trivia, string comment, string header)
    {
        string Text = comment.Substring(header.Length);

        Log($"Require '{Text}' not supported at this location.");

        Location Location = trivia.GetLocation();
        parsingContext.Unsupported.AddUnsupportedRequire(Text, Location);
    }
}
