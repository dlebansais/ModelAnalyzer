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
    private List<IEnsure> ParseEnsures(MethodDeclarationSyntax methodDeclaration, FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported)
    {
        List<IEnsure> EnsureList;

        SyntaxToken LastToken = methodDeclaration.GetLastToken();
        SyntaxToken NextToken = LastToken.GetNextToken();

        if (NextToken.HasLeadingTrivia)
            EnsureList = ParseEnsures(NextToken.LeadingTrivia, fieldTable, parameterTable, unsupported);
        else
            EnsureList = new();

        return EnsureList;
    }

    private List<IEnsure> ParseEnsures(SyntaxTriviaList triviaList, FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported)
    {
        List<IEnsure> EnsureList = new();

        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Ensure}";

                if (Comment.StartsWith(Pattern))
                    ParseEnsure(fieldTable, parameterTable, unsupported, EnsureList, Trivia, Comment, Pattern);
            }

        return EnsureList;
    }

    private void ParseEnsure(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, List<IEnsure> ensureList, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);
        IEnsure NewEnsure;

        Log($"Analyzing ensure '{Text}'.");

        if (TryParseAssertionInTrivia(fieldTable, parameterTable, unsupported, Text, out IExpression BooleanExpression))
        {
            NewEnsure = new Ensure { Text = Text, BooleanExpression = BooleanExpression };

            Log($"Ensure analyzed: '{NewEnsure}'.");
        }
        else
        {
            Location Location = GetLocationInComment(trivia, pattern);
            unsupported.AddUnsupportedEnsure(Text, Location, out IUnsupportedEnsure UnsupportedEnsure);

            NewEnsure = UnsupportedEnsure;
        }

        ensureList.Add(NewEnsure);
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

        Location Location = GetLocationInComment(trivia, pattern);
        unsupported.AddUnsupportedEnsure(Text, Location, out _);
    }
}
