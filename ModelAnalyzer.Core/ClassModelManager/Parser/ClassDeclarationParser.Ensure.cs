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
    private List<Ensure> ParseEnsures(MethodDeclarationSyntax methodDeclaration, ReadOnlyFieldTable fieldTable, Method hostMethod, Local? resultLocal, Unsupported unsupported)
    {
        List<Ensure> EnsureList;

        SyntaxToken LastToken = methodDeclaration.GetLastToken();
        SyntaxToken NextToken = LastToken.GetNextToken();

        if (NextToken.HasLeadingTrivia)
            EnsureList = ParseEnsures(NextToken.LeadingTrivia, fieldTable, hostMethod, resultLocal, unsupported);
        else
            EnsureList = new();

        return EnsureList;
    }

    private List<Ensure> ParseEnsures(SyntaxTriviaList triviaList, ReadOnlyFieldTable fieldTable, Method hostMethod, Local? resultLocal, Unsupported unsupported)
    {
        List<Ensure> EnsureList = new();

        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string EnsureHeader = $"// {Modeling.Ensure}";

                if (Comment.StartsWith(EnsureHeader))
                    ParseEnsure(fieldTable, hostMethod, resultLocal, unsupported, EnsureList, Trivia, Comment, EnsureHeader);
            }

        return EnsureList;
    }

    private void ParseEnsure(ReadOnlyFieldTable fieldTable, Method hostMethod, Local? resultLocal, Unsupported unsupported, List<Ensure> ensureList, SyntaxTrivia trivia, string comment, string header)
    {
        string Text = comment.Substring(header.Length);

        Log($"Analyzing ensure '{Text}'.");

        Ensure? NewEnsure = null;
        bool IsErrorReported = false;

        if (TryParseAssertionTextInTrivia(Text, out SyntaxTree SyntaxTree, out int Offset))
        {
            LocationContext LocationContext = new(trivia, header, Offset);

            if (IsValidAssertionSyntaxTree(fieldTable, hostMethod, isLocalAllowed: false, resultLocal, unsupported, LocationContext, SyntaxTree, out Expression BooleanExpression, out IsErrorReported))
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
            unsupported.AddUnsupportedEnsure(Text, Location);
        }
    }

    private void ReportUnsupportedEnsures(Unsupported unsupported, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Ensure}";

                if (Comment.StartsWith(Pattern))
                    ReportUnsupportedEnsure(unsupported, Trivia, Comment, Pattern);
            }
    }

    private void ReportUnsupportedEnsure(Unsupported unsupported, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);

        Log($"Ensure '{Text}' not supported at this location.");

        Location Location = trivia.GetLocation();
        unsupported.AddUnsupportedEnsure(Text, Location);
    }
}
