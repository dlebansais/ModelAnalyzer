namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private Expression? ParseExpression(ParsingContext parsingContext, ExpressionSyntax expressionNode)
    {
        Debug.Assert(parsingContext.LocationContext is not null);

        bool IsExpressionNested = parsingContext.IsExpressionNested;
        Expression? NewExpression = null;
        Location Location = parsingContext.LocationContext!.GetLocation(expressionNode);
        ParsingContext NestedParsingContext = parsingContext with { IsExpressionNested = true };
        bool IsErrorReported = false;

        if (expressionNode is BinaryExpressionSyntax BinaryExpression)
            NewExpression = TryParseBinaryExpression(NestedParsingContext, BinaryExpression, ref IsErrorReported, ref Location);
        else if (expressionNode is PrefixUnaryExpressionSyntax PrefixUnaryExpression)
            NewExpression = TryParsePrefixUnaryExpression(NestedParsingContext, PrefixUnaryExpression, ref IsErrorReported, ref Location);
        else if (expressionNode is IdentifierNameSyntax IdentifierName)
            NewExpression = TryParseVariableValueExpression(NestedParsingContext, IdentifierName);
        else if (expressionNode is LiteralExpressionSyntax LiteralExpression)
            NewExpression = TryParseLiteralValueExpression(LiteralExpression);
        else if (expressionNode is ParenthesizedExpressionSyntax ParenthesizedExpression)
            NewExpression = TryParseParenthesizedExpression(NestedParsingContext, ParenthesizedExpression, ref IsErrorReported);
        else if (expressionNode is InvocationExpressionSyntax InvocationExpression)
            NewExpression = TryParseFunctionCallExpression(NestedParsingContext, InvocationExpression, ref IsErrorReported);
        else if (expressionNode is ObjectCreationExpressionSyntax ObjectCreationExpression)
            NewExpression = TryParseObjectCreationExpression(NestedParsingContext, ObjectCreationExpression, ref IsErrorReported);
        else
            Log($"Unsupported expression type '{expressionNode.GetType().Name}'.");

        if (NewExpression is null)
        {
            if (!IsErrorReported)
                parsingContext.Unsupported.AddUnsupportedExpression(Location);
        }
        else if (!IsExpressionNested) // Only log the top-level expression.
            Log($"Expression analyzed: '{NewExpression}'.");

        return NewExpression;
    }

    private Expression? TryParseBinaryExpression(ParsingContext parsingContext, BinaryExpressionSyntax binaryExpression, ref bool isErrorReported, ref Location location)
    {
        Expression? NewExpression = null;
        Expression? LeftExpression = ParseExpression(parsingContext, binaryExpression.Left);
        Expression? RightExpression = ParseExpression(parsingContext, binaryExpression.Right);

        if (LeftExpression is Expression Left && RightExpression is Expression Right)
        {
            SyntaxToken OperatorToken = binaryExpression.OperatorToken;

            if (IsSupportedBinaryArithmeticOperator(OperatorToken, out BinaryArithmeticOperator BinaryArithmeticOperator))
                NewExpression = new BinaryArithmeticExpression { Left = Left, Operator = BinaryArithmeticOperator, Right = Right };
            else if (OperatorToken.IsKind(SyntaxKind.PercentToken))
                NewExpression = TryParseRemainderExpression(parsingContext, Left, Right, OperatorToken, ref location);
            else if (IsSupportedBinaryLogicalOperator(OperatorToken, out BinaryLogicalOperator BinaryLogicalOperator))
                NewExpression = new BinaryLogicalExpression { Left = Left, Operator = BinaryLogicalOperator, Right = Right };
            else if (IsSupportedEqualityOperator(OperatorToken, out EqualityOperator EqualityOperator))
                NewExpression = new EqualityExpression { Left = Left, Operator = EqualityOperator, Right = Right };
            else if (IsSupportedComparisonOperator(OperatorToken, out ComparisonOperator ComparisonOperator))
                NewExpression = new ComparisonExpression { Left = Left, Operator = ComparisonOperator, Right = Right };
            else
            {
                Log($"Unsupported operator '{OperatorToken.ValueText}'.");

                Debug.Assert(parsingContext.LocationContext is not null);

                location = parsingContext.LocationContext!.GetLocation(OperatorToken);
            }
        }
        else
            isErrorReported = true;

        return NewExpression;
    }

    private Expression? TryParseRemainderExpression(ParsingContext parsingContext, Expression left, Expression right, SyntaxToken operatorToken, ref Location location)
    {
        Debug.Assert(parsingContext.LocationContext is not null);

        Expression? NewExpression = null;

        if (left.GetExpressionType(parsingContext) != ExpressionType.Integer)
        {
            Log($"'{left}' must be an integer.");

            location = parsingContext.LocationContext!.GetLocation(operatorToken);
        }
        else if (right.GetExpressionType(parsingContext) != ExpressionType.Integer)
        {
            Log($"'{right}' must be an integer.");

            location = parsingContext.LocationContext!.GetLocation(operatorToken);
        }
        else
        {
            NewExpression = new RemainderExpression { Left = left, Right = right };
        }

        return NewExpression;
    }

    private Expression? TryParsePrefixUnaryExpression(ParsingContext parsingContext, PrefixUnaryExpressionSyntax prefixUnaryExpression, ref bool isErrorReported, ref Location location)
    {
        Expression? NewExpression = null;
        Expression? OperandExpression = ParseExpression(parsingContext, prefixUnaryExpression.Operand);

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

                Debug.Assert(parsingContext.LocationContext is not null);

                location = parsingContext.LocationContext!.GetLocation(OperatorToken);
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

    private Expression? TryParseVariableValueExpression(ParsingContext parsingContext, IdentifierNameSyntax identifierName)
    {
        Expression? NewExpression = null;
        string VariableName = identifierName.Identifier.ValueText;

        if (TryFindVariableByName(parsingContext, VariableName, out IVariable Variable))
            NewExpression = new VariableValueExpression { VariableName = Variable.Name };
        else
            Log($"Unknown variable '{VariableName}'.");

        return NewExpression;
    }

    private Expression? TryParseLiteralValueExpression(LiteralExpressionSyntax literalExpression)
    {
        Expression? NewExpression = null;
        string LiteralValue = literalExpression.Token.Text;

        if (LiteralValue == "true")
            NewExpression = new LiteralBooleanValueExpression { Value = true };
        else if (LiteralValue == "false")
            NewExpression = new LiteralBooleanValueExpression { Value = false };
        else if (LiteralValue == "null")
            NewExpression = new LiteralNullExpression();
        else if (int.TryParse(LiteralValue, out int IntegerValue))
            NewExpression = new LiteralIntegerValueExpression { Value = IntegerValue };
        else if (double.TryParse(LiteralValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double FloatingPointValue))
            NewExpression = new LiteralFloatingPointValueExpression { Value = FloatingPointValue };
        else
            Log($"Failed to parse literal value '{LiteralValue}'.");

        return NewExpression;
    }

    private Expression? TryParseParenthesizedExpression(ParsingContext parsingContext, ParenthesizedExpressionSyntax parenthesizedExpression, ref bool isErrorReported)
    {
        Expression? NewExpression = null;
        Expression? NestedExpression = ParseExpression(parsingContext, parenthesizedExpression.Expression);

        if (NestedExpression is not null)
            NewExpression = NestedExpression;
        else
            isErrorReported = true;

        return NewExpression;
    }

    private Expression? TryParseFunctionCallExpression(ParsingContext parsingContext, InvocationExpressionSyntax invocationExpression, ref bool isErrorReported)
    {
        FunctionCallExpression? NewExpression = null;

        if (invocationExpression.Expression is IdentifierNameSyntax IdentifierName)
        {
            MethodName FunctionName = new() { Text = IdentifierName.Identifier.ValueText };
            ArgumentListSyntax InvocationArgumentList = invocationExpression.ArgumentList;
            List<Argument> ArgumentList = TryParseArgumentList(parsingContext, InvocationArgumentList, ref isErrorReported);

            if (ArgumentList.Count == InvocationArgumentList.Arguments.Count)
            {
                NewExpression = new FunctionCallExpression { FunctionName = FunctionName, NameLocation = IdentifierName.GetLocation(), ArgumentList = ArgumentList };

                List<Statement> ParentStatementList = parsingContext.StatementList;
                int OwnerStatementIndex = ParentStatementList.Count;

                parsingContext.FunctionCallExpressionList.Add(new FunctionCallStatementEntry() { HostMethod = parsingContext.HostMethod, Expression = NewExpression, OwnerStatementIndex = OwnerStatementIndex, ParentStatementList = ParentStatementList });
            }
        }

        return NewExpression;
    }

    private Expression? TryParseObjectCreationExpression(ParsingContext parsingContext, ObjectCreationExpressionSyntax objectCreationExpression, ref bool isErrorReported)
    {
        NewObjectExpression? NewExpression = null;
        bool HasArguments = false;
        bool HasInitializer = false;

        if (objectCreationExpression.ArgumentList is ArgumentListSyntax ArgumentList)
            if (ArgumentList.Arguments.Count > 0)
                HasArguments = true;

        if (objectCreationExpression.Initializer is not null)
            HasInitializer = true;

        if (!HasArguments && !HasInitializer)
            if (IsTypeSupported(parsingContext, objectCreationExpression.Type, out ExpressionType ObjectType))
                NewExpression = new NewObjectExpression() { ObjectType = ObjectType };

        return NewExpression;
    }
}
