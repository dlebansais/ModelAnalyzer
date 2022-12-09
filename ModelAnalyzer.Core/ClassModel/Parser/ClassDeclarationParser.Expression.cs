namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private IExpression ParseExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionSyntax expressionNode, bool isNested)
    {
        IExpression? NewExpression = null;
        Location Location = expressionNode.GetLocation();

        if (expressionNode is BinaryExpressionSyntax BinaryExpression)
            NewExpression = TryParseBinaryExpression(fieldTable, parameterTable, unsupported, BinaryExpression, ref Location);
        else if (expressionNode is IdentifierNameSyntax IdentifierName)
            NewExpression = TryParseVariableValueExpression(fieldTable, parameterTable, IdentifierName);
        else if (expressionNode is LiteralExpressionSyntax LiteralExpression)
            NewExpression = TryParseLiteralValueExpression(LiteralExpression);
        else if (expressionNode is ParenthesizedExpressionSyntax ParenthesizedExpression)
            NewExpression = TryParseParenthesizedExpression(fieldTable, parameterTable, unsupported, ParenthesizedExpression);

        if (NewExpression is null)
        {
            unsupported.AddUnsupportedExpression(Location, out IUnsupportedExpression UnsupportedExpression);

            NewExpression = UnsupportedExpression;
        }
        else if (!isNested) // Only log the top-level expression.
            Log($"Expression analyzed: '{NewExpression}'.");

        return NewExpression;
    }

    private IExpression? TryParseBinaryExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, BinaryExpressionSyntax binaryExpression, ref Location location)
    {
        IExpression? NewExpression = null;
        IExpression LeftExpression = ParseExpression(fieldTable, parameterTable, unsupported, binaryExpression.Left, isNested: true);
        IExpression RightExpression = ParseExpression(fieldTable, parameterTable, unsupported, binaryExpression.Right, isNested: true);

        if (LeftExpression is Expression Left && RightExpression is Expression Right)
        {
            SyntaxToken OperatorToken = binaryExpression.OperatorToken;

            if (IsSupportedBinaryArithmeticOperator(OperatorToken, out ArithmeticOperator ArithmeticOperator))
                NewExpression = new BinaryArithmeticExpression { Left = Left, Operator = ArithmeticOperator, Right = Right };
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
        else
            Log($"Unsupported expression type '{binaryExpression.GetType().Name}'.");

        return NewExpression;
    }

    private bool IsSupportedBinaryArithmeticOperator(SyntaxToken token, out ArithmeticOperator arithmeticOperator)
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

    private bool IsSupportedComparisonOperator(SyntaxToken token, out ComparisonOperator comparisonOperator)
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

    private bool IsSupportedBinaryConditionalOperator(SyntaxToken token, out LogicalOperator logicalOperator)
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

    private IExpression? TryParseVariableValueExpression(FieldTable fieldTable, ParameterTable parameterTable, IdentifierNameSyntax identifierName)
    {
        IExpression? NewExpression = null;
        string VariableName = identifierName.Identifier.ValueText;

        if (TryFindVariableByName(fieldTable, parameterTable, VariableName, out IVariable Variable))
            NewExpression = new VariableValueExpression { Variable = Variable };
        else
            Log($"Unknown variable '{VariableName}'.");

        return NewExpression;
    }

    private IExpression? TryParseLiteralValueExpression(LiteralExpressionSyntax literalExpression)
    {
        IExpression? NewExpression = null;
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

    private IExpression? TryParseParenthesizedExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ParenthesizedExpressionSyntax parenthesizedExpression)
    {
        IExpression? NewExpression = null;
        IExpression InsideExpression = ParseExpression(fieldTable, parameterTable, unsupported, parenthesizedExpression.Expression, isNested: true);

        if (InsideExpression is Expression Inside)
            NewExpression = new ParenthesizedExpression { Inside = Inside };

        return NewExpression;
    }
}
