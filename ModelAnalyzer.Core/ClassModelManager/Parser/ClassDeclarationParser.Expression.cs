namespace ModelAnalyzer;

using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private Expression? ParseExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, LocationContext locationContext, ExpressionSyntax expressionNode, bool isNested)
    {
        Expression? NewExpression = null;
        Location Location = locationContext.GetLocation(expressionNode);
        bool IsErrorReported = false;

        if (expressionNode is BinaryExpressionSyntax BinaryExpression)
            NewExpression = TryParseBinaryExpression(fieldTable, parameterTable, unsupported, locationContext, BinaryExpression, ref IsErrorReported, ref Location);
        else if (expressionNode is PrefixUnaryExpressionSyntax PrefixUnaryExpression)
            NewExpression = TryParsePrefixUnaryExpression(fieldTable, parameterTable, unsupported, locationContext, PrefixUnaryExpression, ref IsErrorReported, ref Location);
        else if (expressionNode is IdentifierNameSyntax IdentifierName)
            NewExpression = TryParseVariableValueExpression(fieldTable, parameterTable, IdentifierName);
        else if (expressionNode is LiteralExpressionSyntax LiteralExpression)
            NewExpression = TryParseLiteralValueExpression(LiteralExpression);
        else if (expressionNode is ParenthesizedExpressionSyntax ParenthesizedExpression)
            NewExpression = TryParseParenthesizedExpression(fieldTable, parameterTable, unsupported, locationContext, ParenthesizedExpression, ref IsErrorReported);
        else
            Log($"Unsupported expression type '{expressionNode.GetType().Name}'.");

        if (NewExpression is null)
        {
            if (!IsErrorReported)
                unsupported.AddUnsupportedExpression(Location);
        }
        else if (!isNested) // Only log the top-level expression.
            Log($"Expression analyzed: '{NewExpression}'.");

        return NewExpression;
    }

    private Expression? TryParseBinaryExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, LocationContext locationContext, BinaryExpressionSyntax binaryExpression, ref bool isErrorReported, ref Location location)
    {
        Expression? NewExpression = null;
        Expression? LeftExpression = ParseExpression(fieldTable, parameterTable, unsupported, locationContext, binaryExpression.Left, isNested: true);
        Expression? RightExpression = ParseExpression(fieldTable, parameterTable, unsupported, locationContext, binaryExpression.Right, isNested: true);

        if (LeftExpression is Expression Left && RightExpression is Expression Right)
        {
            SyntaxToken OperatorToken = binaryExpression.OperatorToken;

            if (IsSupportedBinaryArithmeticOperator(OperatorToken, out BinaryArithmeticOperator BinaryArithmeticOperator))
                NewExpression = new BinaryArithmeticExpression { Left = Left, Operator = BinaryArithmeticOperator, Right = Right };
            else if (IsSupportedBinaryLogicalOperator(OperatorToken, out BinaryLogicalOperator BinaryLogicalOperator))
                NewExpression = new BinaryLogicalExpression { Left = Left, Operator = BinaryLogicalOperator, Right = Right };
            else if (IsSupportedEqualityOperator(OperatorToken, out EqualityOperator EqualityOperator))
                NewExpression = new EqualityExpression { Left = Left, Operator = EqualityOperator, Right = Right };
            else if (IsSupportedComparisonOperator(OperatorToken, out ComparisonOperator ComparisonOperator))
                NewExpression = new ComparisonExpression { Left = Left, Operator = ComparisonOperator, Right = Right };
            else
            {
                Log($"Unsupported operator '{OperatorToken.ValueText}'.");

                location = locationContext.GetLocation(OperatorToken);
            }
        }
        else
            isErrorReported = true;

        return NewExpression;
    }

    private Expression? TryParsePrefixUnaryExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, LocationContext locationContext, PrefixUnaryExpressionSyntax prefixUnaryExpression, ref bool isErrorReported, ref Location location)
    {
        Expression? NewExpression = null;
        Expression? OperandExpression = ParseExpression(fieldTable, parameterTable, unsupported, locationContext, prefixUnaryExpression.Operand, isNested: true);

        if (OperandExpression is Expression Operand)
        {
            SyntaxToken OperatorToken = prefixUnaryExpression.OperatorToken;

            if (IsSupportedUnaryArithmeticOperator(OperatorToken, out UnaryArithmeticOperator UnaryArithmeticOperator))
                NewExpression = new UnaryArithmeticExpression { Operator = UnaryArithmeticOperator, Operand = Operand };
            else if (IsSupportedUnaryLogicalOperator(OperatorToken, out UnaryLogicalOperator UnaryLogicalOperator))
                NewExpression = new UnaryLogicalExpression { Operator = UnaryLogicalOperator, Operand = Operand };
            else
            {
                Log($"Unsupported operator '{OperatorToken.ValueText}'.");

                location = locationContext.GetLocation(OperatorToken);
            }
        }
        else
            isErrorReported = true;

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

    private bool IsSupportedUnaryLogicalOperator(SyntaxToken token, out UnaryLogicalOperator logicalOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (OperatorSyntaxKind.UnaryLogical.ContainsKey(OperatorKind))
        {
            logicalOperator = OperatorSyntaxKind.UnaryLogical[OperatorKind];
            return true;
        }

        logicalOperator = default;
        return false;
    }

    private bool IsSupportedEqualityOperator(SyntaxToken token, out EqualityOperator equalityOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (OperatorSyntaxKind.Equality.ContainsKey(OperatorKind))
        {
            equalityOperator = OperatorSyntaxKind.Equality[OperatorKind];
            return true;
        }

        equalityOperator = default;
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

    private bool IsSupportedBinaryLogicalOperator(SyntaxToken token, out BinaryLogicalOperator logicalOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (OperatorSyntaxKind.BinaryLogical.ContainsKey(OperatorKind))
        {
            logicalOperator = OperatorSyntaxKind.BinaryLogical[OperatorKind];
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
        else if (int.TryParse(LiteralValue, out int IntValue))
            NewExpression = new LiteralIntValueExpression { Value = IntValue };
        else if (double.TryParse(LiteralValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double DoubleValue))
            NewExpression = new LiteralFloatingPointValueExpression { Value = DoubleValue };
        else
            Log($"Failed to parse literal value '{LiteralValue}'.");

        return NewExpression;
    }

    private Expression? TryParseParenthesizedExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, LocationContext locationContext, ParenthesizedExpressionSyntax parenthesizedExpression, ref bool isErrorReported)
    {
        Expression? NewExpression = null;
        Expression? NestedExpression = ParseExpression(fieldTable, parameterTable, unsupported, locationContext, parenthesizedExpression.Expression, isNested: true);

        if (NestedExpression is not null)
            NewExpression = NestedExpression;
        else
            isErrorReported = true;

        return NewExpression;
    }
}
