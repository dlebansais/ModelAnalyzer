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
    private List<Ensure> ParseEnsures(ParsingContext parsingContext, MethodDeclarationSyntax methodDeclaration, Local? resultLocal)
    {
        List<Ensure> EnsureList;

        SyntaxToken LastToken = methodDeclaration.GetLastToken();
        SyntaxToken NextToken = LastToken.GetNextToken();

        if (NextToken.HasLeadingTrivia)
            EnsureList = ParseEnsures(parsingContext, NextToken.LeadingTrivia, resultLocal);
        else
            EnsureList = new();

        return EnsureList;
    }

    private List<Ensure> ParseEnsures(ParsingContext parsingContext, SyntaxTriviaList triviaList, Local? resultLocal)
    {
        List<Ensure> EnsureList = new();

        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string EnsureHeader = $"// {Modeling.Ensure}";

                if (Comment.StartsWith(EnsureHeader))
                    ParseEnsure(parsingContext, resultLocal, EnsureList, Trivia, Comment, EnsureHeader);
            }

        return EnsureList;
    }

    private void ParseEnsure(ParsingContext parsingContext, Local? resultLocal, List<Ensure> ensureList, SyntaxTrivia trivia, string comment, string header)
    {
        string Text = comment.Substring(header.Length);

        Log($"Analyzing ensure '{Text}'.");

        Ensure? NewEnsure = null;
        bool IsErrorReported = false;

        if (TryParseAssertionTextInTrivia(Text, out SyntaxTree SyntaxTree, out int Offset))
        {
            LocationContext LocationContext = new(trivia, header, Offset);
            ParsingContext EnsureParsingContext = parsingContext with { IsLocalAllowed = false, ResultLocal = resultLocal, LocationContext = LocationContext };

            if (IsValidAssertionSyntaxTree(EnsureParsingContext, SyntaxTree, out Expression BooleanExpression, out IsErrorReported))
            {
                NewEnsure = new Ensure { Text = Text, Location = trivia.GetLocation(), BooleanExpression = BooleanExpression };
            }
        }

        if (NewEnsure is not null)
        {
            Log($"Ensure analyzed: '{NewEnsure}'.");

            ensureList.Add(NewEnsure);
        }
        else if (!IsErrorReported)
        {
            Location Location = trivia.GetLocation();
            parsingContext.Unsupported.AddUnsupportedEnsure(Text, Location);
        }
    }

    private void ReportUnsupportedEnsures(ParsingContext parsingContext, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Ensure}";

                if (Comment.StartsWith(Pattern))
                    ReportUnsupportedEnsure(parsingContext, Trivia, Comment, Pattern);
            }
    }

    private void ReportUnsupportedEnsure(ParsingContext parsingContext, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);

        Log($"Ensure '{Text}' not supported at this location.");

        Location Location = trivia.GetLocation();
        parsingContext.Unsupported.AddUnsupportedEnsure(Text, Location);
    }
}
