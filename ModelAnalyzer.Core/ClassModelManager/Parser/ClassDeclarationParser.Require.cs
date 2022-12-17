﻿namespace ModelAnalyzer;

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
                string Pattern = $"// {Modeling.Require}";

                if (Comment.StartsWith(Pattern))
                    ParseRequire(fieldTable, parameterTable, unsupported, RequireList, Trivia, Comment, Pattern);
            }

        return RequireList;
    }

    private void ParseRequire(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, List<Require> requireList, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);

        Log($"Analyzing require '{Text}'.");

        if (TryParseAssertionInTrivia(fieldTable, parameterTable, unsupported, Text, out Expression BooleanExpression))
        {
            Require NewRequire = new Require { Text = Text, BooleanExpression = BooleanExpression };

            Log($"Require analyzed: '{NewRequire}'.");

            requireList.Add(NewRequire);
        }
        else
        {
            Location Location = GetLocationInComment(trivia, pattern);
            unsupported.AddUnsupportedRequire(Text, Location, out UnsupportedRequire UnsupportedRequire);
        }
    }

    private void ReportUnsupportedRequires(Unsupported unsupported, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Require}";

                if (Comment.StartsWith(Pattern))
                    ReportUnsupportedRequire(unsupported, Trivia, Comment, Pattern);
            }
    }

    private void ReportUnsupportedRequire(Unsupported unsupported, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);

        Log($"Require '{Text}' not supported at this location.");

        Location Location = GetLocationInComment(trivia, pattern);
        unsupported.AddUnsupportedRequire(Text, Location, out _);
    }
}