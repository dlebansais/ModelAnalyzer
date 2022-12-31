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
    private List<Require> ParseRequires(MethodDeclarationSyntax methodDeclaration, ReadOnlyFieldTable fieldTable, Method hostMethod, Unsupported unsupported)
    {
        List<Require> RequireList;

        if (methodDeclaration.Body is BlockSyntax Block && Block.HasLeadingTrivia)
            RequireList = ParseRequires(Block.GetLeadingTrivia(), fieldTable, hostMethod, unsupported);
        else
            RequireList = new();

        return RequireList;
    }

    private List<Require> ParseRequires(SyntaxTriviaList triviaList, ReadOnlyFieldTable fieldTable, Method hostMethod, Unsupported unsupported)
    {
        List<Require> RequireList = new();

        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string RequireHeader = $"// {Modeling.Require}";
                string EnsureHeader = $"// {Modeling.Ensure}";

                if (Comment.StartsWith(RequireHeader))
                    ParseRequire(fieldTable, hostMethod, unsupported, RequireList, Trivia, Comment, RequireHeader);
                else if (Comment.StartsWith(EnsureHeader))
                    ReportUnsupportedEnsure(unsupported, Trivia, Comment, EnsureHeader);
            }

        return RequireList;
    }

    private void ParseRequire(ReadOnlyFieldTable fieldTable, Method hostMethod, Unsupported unsupported, List<Require> requireList, SyntaxTrivia trivia, string comment, string header)
    {
        string Text = comment.Substring(header.Length);

        Log($"Analyzing require '{Text}'.");

        Require? NewRequire = null;
        bool IsErrorReported = false;

        if (TryParseAssertionTextInTrivia(Text, out SyntaxTree SyntaxTree, out int Offset))
        {
            LocationContext LocationContext = new(trivia, header, Offset);

            if (IsValidAssertionSyntaxTree(fieldTable, hostMethod, isLocalAllowed: false, resultLocal: null, unsupported, LocationContext, SyntaxTree, out Expression BooleanExpression, out IsErrorReported))
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
            unsupported.AddUnsupportedRequire(Text, Location);
        }
    }

    private void ReportUnsupportedRequires(Unsupported unsupported, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Header = $"// {Modeling.Require}";

                if (Comment.StartsWith(Header))
                    ReportUnsupportedRequire(unsupported, Trivia, Comment, Header);
            }
    }

    private void ReportUnsupportedRequire(Unsupported unsupported, SyntaxTrivia trivia, string comment, string header)
    {
        string Text = comment.Substring(header.Length);

        Log($"Require '{Text}' not supported at this location.");

        Location Location = trivia.GetLocation();
        unsupported.AddUnsupportedRequire(Text, Location);
    }
}
