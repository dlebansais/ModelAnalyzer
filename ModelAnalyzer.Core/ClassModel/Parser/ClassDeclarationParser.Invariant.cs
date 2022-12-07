namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Linq;
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

        var NextToken = classDeclaration.SyntaxTree.GetRoot().FindToken(Location.SourceSpan.End);

        if (NextToken.HasLeadingTrivia)
            AddInvariantsInTrivia(InvariantList, fieldTable, unsupported, NextToken.LeadingTrivia);

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

    private bool IsValidAssertionSyntaxTree(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, SyntaxTree syntaxTree, out IExpression booleanExpression)
    {
        bool IsInvariantSupported = true;

        CompilationUnitSyntax Root = syntaxTree.GetCompilationUnitRoot();

        if (Root.AttributeLists.Count > 0)
        {
            Log($"Attributes not supported in assertions.");
            IsInvariantSupported = false;
        }

        if (Root.Usings.Count > 0)
        {
            Log($"Using directive not supported in assertions.");
            IsInvariantSupported = false;
        }

        booleanExpression = null!;

        if (Root.Members.Count != 1)
        {
            Log($"There can be only one expression in an assertion.");
            IsInvariantSupported = false;
        }
        else if (Root.Members[0] is not GlobalStatementSyntax GlobalStatement ||
                 GlobalStatement.Statement is not ExpressionStatementSyntax ExpressionStatement ||
                 ExpressionStatement.Expression is not AssignmentExpressionSyntax AssignmentExpression)
        {
            Log($"Unsupported assertion syntax.");
            IsInvariantSupported = false;
        }
        else
        {
            ExpressionSyntax Expression = AssignmentExpression.Right;

            if (!IsValidInvariantExpression(fieldTable, parameterTable, unsupported, Expression, out booleanExpression))
                IsInvariantSupported = false;
        }

        return IsInvariantSupported;
    }

    private bool IsValidInvariantExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionSyntax expressionNode, out IExpression booleanExpression)
    {
        booleanExpression = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode);

        return booleanExpression is not UnsupportedExpression;
    }
}
