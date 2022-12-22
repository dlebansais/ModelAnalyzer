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
    private List<Require> ParseRequires(MethodDeclarationSyntax methodDeclaration, FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported)
    {
        List<Require> RequireList;

        if (methodDeclaration.Body is BlockSyntax Block && Block.HasLeadingTrivia)
            RequireList = ParseRequires(Block.GetLeadingTrivia(), fieldTable, parameterTable, unsupported);
        else
            RequireList = new();

        return RequireList;
    }

    private List<Require> ParseRequires(SyntaxTriviaList triviaList, FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported)
    {
        List<Require> RequireList = new();

        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Header = $"// {Modeling.Require}";

                if (Comment.StartsWith(Header))
                    ParseRequire(fieldTable, parameterTable, unsupported, RequireList, Trivia, Comment, Header);
            }

        return RequireList;
    }

    private void ParseRequire(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, List<Require> requireList, SyntaxTrivia trivia, string comment, string header)
    {
        string Text = comment.Substring(header.Length);

        Log($"Analyzing require '{Text}'.");

        LocationContext LocationContext = new(trivia, header, AssignmentAssertionText.Length);

        if (TryParseAssertionInTrivia(fieldTable, parameterTable, unsupported, Text, LocationContext, out Expression BooleanExpression, out bool IsErrorReported))
        {
            Require NewRequire = new Require { Text = Text, BooleanExpression = BooleanExpression };

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
