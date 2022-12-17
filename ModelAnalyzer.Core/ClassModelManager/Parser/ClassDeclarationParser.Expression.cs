﻿namespace ModelAnalyzer;

using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private Expression? ParseExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionSyntax expressionNode, bool isNested)
    {
        Expression? NewExpression = null;
        Location Location = expressionNode.GetLocation();

        if (expressionNode is BinaryExpressionSyntax BinaryExpression)
            NewExpression = TryParseBinaryExpression(fieldTable, parameterTable, unsupported, BinaryExpression, ref Location);
        else if (expressionNode is PrefixUnaryExpressionSyntax PrefixUnaryExpression)
            NewExpression = TryParsePrefixUnaryExpression(fieldTable, parameterTable, unsupported, PrefixUnaryExpression, ref Location);
        else if (expressionNode is IdentifierNameSyntax IdentifierName)
            NewExpression = TryParseVariableValueExpression(fieldTable, parameterTable, IdentifierName);
        else if (expressionNode is LiteralExpressionSyntax LiteralExpression)
            NewExpression = TryParseLiteralValueExpression(LiteralExpression);
        else if (expressionNode is ParenthesizedExpressionSyntax ParenthesizedExpression)
            NewExpression = TryParseParenthesizedExpression(fieldTable, parameterTable, unsupported, ParenthesizedExpression);
        else
            Log($"Unsupported expression type '{expressionNode.GetType().Name}'.");

        if (NewExpression is null)
        {
            unsupported.AddUnsupportedExpression(Location, out UnsupportedExpression UnsupportedExpression);
        }
        else if (!isNested) // Only log the top-level expression.
            Log($"Expression analyzed: '{NewExpression}'.");

        return NewExpression;
    }

    private Expression? TryParseBinaryExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, BinaryExpressionSyntax binaryExpression, ref Location location)
    {
        Expression? NewExpression = null;
        Expression? LeftExpression = ParseExpression(fieldTable, parameterTable, unsupported, binaryExpression.Left, isNested: true);
        Expression? RightExpression = ParseExpression(fieldTable, parameterTable, unsupported, binaryExpression.Right, isNested: true);

        if (LeftExpression is Expression Left && RightExpression is Expression Right)
        {
            SyntaxToken OperatorToken = binaryExpression.OperatorToken;

            if (IsSupportedBinaryArithmeticOperator(OperatorToken, out BinaryArithmeticOperator BinaryArithmeticOperator))
                NewExpression = new BinaryArithmeticExpression { Left = Left, Operator = BinaryArithmeticOperator, Right = Right };
            else if (IsSupportedComparisonOperator(OperatorToken, out ComparisonOperator ComparisonOperator))
                NewExpression = new ComparisonExpression { Left = Left, Operator = ComparisonOperator, Right = Right };
            else if (IsSupportedBinaryConditionalOperator(OperatorToken, out LogicalOperator LogicalOperator))
                NewExpression = new BinaryConditionalExpression { Left = Left, Operator = LogicalOperator, Right = Right };
            else
            {
                Log($"Unsupported operator '{OperatorToken.ValueText}'.");

                location = OperatorToken.GetLocation();
            }
        }

        return NewExpression;
    }

    private Expression? TryParsePrefixUnaryExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, PrefixUnaryExpressionSyntax prefixUnaryExpression, ref Location location)
    {
        Expression? NewExpression = null;
        Expression? OperandExpression = ParseExpression(fieldTable, parameterTable, unsupported, prefixUnaryExpression.Operand, isNested: true);

        if (OperandExpression is Expression Operand)
        {
            SyntaxToken OperatorToken = prefixUnaryExpression.OperatorToken;

            if (IsSupportedUnaryArithmeticOperator(OperatorToken, out UnaryArithmeticOperator UnaryArithmeticOperator))
                NewExpression = new UnaryArithmeticExpression { Operator = UnaryArithmeticOperator, Operand = Operand };
            else
            {
                Log($"Unsupported operator '{OperatorToken.ValueText}'.");

                location = OperatorToken.GetLocation();
            }
        }
        else
            Log($"Unsupported expression type '{prefixUnaryExpression.GetType().Name}'.");

        return NewExpression;
    }

    private bool IsSupportedBinaryArithmeticOperator(SyntaxToken token, out BinaryArithmeticOperator arithmeticOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (OperatorSyntaxKind.BinaryArithmetic.ContainsKey(OperatorKind))
        {
            arithmeticOperator = OperatorSyntaxKind.BinaryArithmetic[OperatorKind];
            return true;
        }

        arithmeticOperator = default;
        return false;
    }

    private bool IsSupportedUnaryArithmeticOperator(SyntaxToken token, out UnaryArithmeticOperator arithmeticOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (OperatorSyntaxKind.UnaryArithmetic.ContainsKey(OperatorKind))
        {
            arithmeticOperator = OperatorSyntaxKind.UnaryArithmetic[OperatorKind];
            return true;
        }

        arithmeticOperator = default;
        return false;
    }

    private bool IsSupportedComparisonOperator(SyntaxToken token, out ComparisonOperator comparisonOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (OperatorSyntaxKind.Comparison.ContainsKey(OperatorKind))
        {
            comparisonOperator = OperatorSyntaxKind.Comparison[OperatorKind];
            return true;
        }

        comparisonOperator = default;
        return false;
    }

    private bool IsSupportedBinaryConditionalOperator(SyntaxToken token, out LogicalOperator logicalOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (OperatorSyntaxKind.Logical.ContainsKey(OperatorKind))
        {
            logicalOperator = OperatorSyntaxKind.Logical[OperatorKind];
            return true;
        }

        logicalOperator = default;
        return false;
    }

    private Expression? TryParseVariableValueExpression(FieldTable fieldTable, ParameterTable parameterTable, IdentifierNameSyntax identifierName)
    {
        Expression? NewExpression = null;
        string VariableName = identifierName.Identifier.ValueText;

        if (TryFindVariableByName(fieldTable, parameterTable, VariableName, out IVariable Variable))
            NewExpression = new VariableValueExpression { Variable = Variable };
        else
            Log($"Unknown variable '{VariableName}'.");

        return NewExpression;
    }

    private Expression? TryParseLiteralValueExpression(LiteralExpressionSyntax literalExpression)
    {
        Expression? NewExpression = null;
        string LiteralValue = literalExpression.Token.ValueText;

        if (LiteralValue == "true")
            NewExpression = new LiteralBoolValueExpression { Value = true };
        else if (LiteralValue == "false")
            NewExpression = new LiteralBoolValueExpression { Value = false };
        else if (int.TryParse(LiteralValue, out int Value))
            NewExpression = new LiteralIntValueExpression { Value = Value };
        else
            Log($"Failed to parse literal value '{LiteralValue}'.");

        return NewExpression;
    }

    private Expression? TryParseParenthesizedExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        Expression? NewExpression = null;
        Expression? InsideExpression = ParseExpression(fieldTable, parameterTable, unsupported, parenthesizedExpression.Expression, isNested: true);

        if (InsideExpression is Expression Inside)
            NewExpression = new ParenthesizedExpression { Inside = Inside };

        return NewExpression;
    }
}