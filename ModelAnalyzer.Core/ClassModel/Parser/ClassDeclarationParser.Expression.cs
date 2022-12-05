namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private IExpression ParseExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionSyntax expressionNode)
    {
        IExpression NewExpression;

        if (expressionNode is BinaryExpressionSyntax BinaryExpression)
            NewExpression = ParseBinaryExpression(fieldTable, parameterTable, unsupported, BinaryExpression);
        else if (expressionNode is IdentifierNameSyntax IdentifierName && TryFindVariableByName(fieldTable, parameterTable, IdentifierName.Identifier.ValueText, out IVariable Variable))
            NewExpression = new VariableValueExpression { Variable = Variable };
        else if (expressionNode is LiteralExpressionSyntax LiteralExpression && int.TryParse(LiteralExpression.Token.ValueText, out int Value))
            NewExpression = new LiteralValueExpression { Value = Value };
        else if (expressionNode is ParenthesizedExpressionSyntax ParenthesizedExpression)
            NewExpression = ParseParenthesizedExpression(fieldTable, parameterTable, unsupported, ParenthesizedExpression);
        else
        {
            Location Location = expressionNode.GetLocation();
            unsupported.AddUnsupportedExpression(Location, out IUnsupportedExpression UnsupportedExpression);

            NewExpression = UnsupportedExpression;
        }

        return NewExpression;
    }

    private IExpression ParseBinaryExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, BinaryExpressionSyntax expressionNode)
    {
        IExpression? NewExpression = null;
        IExpression LeftExpression = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode.Left);
        IExpression RightExpression = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode.Right);

        if (LeftExpression is Expression Left && RightExpression is Expression Right)
        {
            if (IsBinaryArithmeticOperatorSupported(expressionNode.OperatorToken, out ArithmeticOperator ArithmeticOperator))
                NewExpression = new BinaryArithmeticExpression { Left = Left, Operator = ArithmeticOperator, Right = Right };
            else if (IsComparisonOperatorSupported(expressionNode.OperatorToken, out ComparisonOperator ComparisonOperator))
                NewExpression = new ComparisonExpression { Left = Left, Operator = ComparisonOperator, Right = Right };
            else if (IsBinaryLogicalOperatorSupported(expressionNode.OperatorToken, out LogicalOperator LogicalOperator))
                NewExpression = new BinaryLogicalExpression { Left = Left, Operator = LogicalOperator, Right = Right };
            else
                Log($"Unsupported expression type '{expressionNode.GetType().Name}'.");
        }

        if (NewExpression is null)
        {
            Location Location = expressionNode.OperatorToken.GetLocation();
            unsupported.AddUnsupportedExpression(Location, out IUnsupportedExpression UnsupportedExpression);

            NewExpression = UnsupportedExpression;
        }

        return NewExpression;
    }

    private bool IsBinaryArithmeticOperatorSupported(SyntaxToken token, out ArithmeticOperator arithmeticOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (SupportedOperators.Arithmetic.ContainsKey(OperatorKind))
        {
            arithmeticOperator = SupportedOperators.Arithmetic[OperatorKind];
            return true;
        }
        else
            Log($"Unsupported arithmetic operator '{token.ValueText}'.");

        arithmeticOperator = null!;
        return false;
    }

    private bool IsComparisonOperatorSupported(SyntaxToken token, out ComparisonOperator comparisonOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (SupportedOperators.Comparison.ContainsKey(OperatorKind))
        {
            comparisonOperator = SupportedOperators.Comparison[OperatorKind];
            return true;
        }
        else
            Log($"Unsupported comparison operator '{token.ValueText}'.");

        comparisonOperator = null!;
        return false;
    }

    private bool IsBinaryLogicalOperatorSupported(SyntaxToken token, out LogicalOperator logicalOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (SupportedOperators.Logical.ContainsKey(OperatorKind))
        {
            logicalOperator = SupportedOperators.Logical[OperatorKind];
            return true;
        }
        else
            Log($"Unsupported logical operator '{token.ValueText}'.");

        logicalOperator = null!;
        return false;
    }

    private IExpression ParseParenthesizedExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ParenthesizedExpressionSyntax expressionNode)
    {
        IExpression NewExpression;
        IExpression InsideExpression = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode.Expression);

        if (InsideExpression is Expression Inside)
        {
            NewExpression = new ParenthesizedExpression { Inside = Inside };
        }
        else
        {
            Location Location = expressionNode.Expression.GetLocation();
            unsupported.AddUnsupportedExpression(Location, out IUnsupportedExpression UnsupportedExpression);

            NewExpression = UnsupportedExpression;
        }

        return NewExpression;
    }
}
