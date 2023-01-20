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
        else if (expressionNode is MemberAccessExpressionSyntax MemberAccessExpression)
            NewExpression = TryParseMemberAccessExpression(NestedParsingContext, MemberAccessExpression);
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

        if (left.GetExpressionType() != ExpressionType.Integer)
        {
            Log($"'{left}' must be an integer.");

            location = parsingContext.LocationContext!.GetLocation(operatorToken);
        }
        else if (right.GetExpressionType() != ExpressionType.Integer)
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
        {
            VariableValueExpression NewVariableValueExpression = new VariableValueExpression { VariablePath = new List<IVariable>() { Variable }, PathLocation = identifierName.GetLocation() };
            NewExpression = NewVariableValueExpression;
        }
        else
            Log($"Unknown variable '{VariableName}'.");

        return NewExpression;
    }

    private Expression? TryParseMemberAccessExpression(ParsingContext parsingContext, MemberAccessExpressionSyntax memberAccessExpression)
    {
        Expression? NewExpression = null;

        if (TryParsePropertyPath(parsingContext, memberAccessExpression, out List<IVariable> VariablePath, out string LastName, out Location PathLocation))
            if (TryParseLastNameAsProperty(parsingContext, VariablePath, LastName, out Property LastProperty))
            {
                VariablePath.Add(LastProperty);

                VariableValueExpression NewVariableValueExpression = new VariableValueExpression { VariablePath = VariablePath, PathLocation = PathLocation };
                NewExpression = NewVariableValueExpression;
            }

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
        Expression? NewExpression = null;

        List<Argument> ArgumentList = TryParseArgumentList(parsingContext, invocationExpression.ArgumentList, ref isErrorReported);

        if (ArgumentList.Count == invocationExpression.ArgumentList.Arguments.Count)
        {
            if (invocationExpression.Expression is IdentifierNameSyntax IdentifierName)
                NewExpression = TryParsePrivateFunctionCallExpression(parsingContext, IdentifierName, ArgumentList, ref isErrorReported);
            else if (invocationExpression.Expression is MemberAccessExpressionSyntax MemberAccessExpression)
                NewExpression = TryParsePublicFunctionCallExpression(parsingContext, MemberAccessExpression, ArgumentList, ref isErrorReported);
            else
                Log("Unsupported function name.");
        }

        return NewExpression;
    }

    private PrivateFunctionCallExpression TryParsePrivateFunctionCallExpression(ParsingContext parsingContext, IdentifierNameSyntax identifierName, List<Argument> argumentList, ref bool isErrorReported)
    {
        MethodName FunctionName = new() { Text = identifierName.Identifier.ValueText };

        ExpressionType ReturnType = ExpressionType.Other;

        string ClassName = parsingContext.ClassName;
        Dictionary<string, IClassModel> Phase1ClassModelTable = parsingContext.SemanticModel.Phase1ClassModelTable;

        Debug.Assert(Phase1ClassModelTable.ContainsKey(ClassName));
        IClassModel ClassModel = Phase1ClassModelTable[ClassName];

        foreach (Method Method in ClassModel.GetMethods())
            if (Method.Name.Text == FunctionName.Text)
            {
                ReturnType = Method.ReturnType;
                break;
            }

        PrivateFunctionCallExpression NewExpression = new() { Name = FunctionName, ReturnType = ReturnType, NameLocation = identifierName.GetLocation(), ArgumentList = argumentList };
        AddFunctionCallEntry(parsingContext, NewExpression);

        return NewExpression;
    }

    private PublicFunctionCallExpression? TryParsePublicFunctionCallExpression(ParsingContext parsingContext, MemberAccessExpressionSyntax memberAccessExpression, List<Argument> argumentList, ref bool isErrorReported)
    {
        PublicFunctionCallExpression? NewExpression = null;

        if (TryParsePropertyPath(parsingContext, memberAccessExpression, out List<IVariable> VariablePath, out string LastName, out Location PathLocation))
            if (TryParseLastNameAsMethod(parsingContext, VariablePath, LastName, out Method CalledMethod))
            {
                NewExpression = new PublicFunctionCallExpression { VariablePath = VariablePath, Name = CalledMethod.Name, ReturnType = CalledMethod.ReturnType, NameLocation = PathLocation, ArgumentList = argumentList };
                AddFunctionCallEntry(parsingContext, NewExpression);
            }

        return NewExpression;
    }

    private void AddFunctionCallEntry(ParsingContext parsingContext, IFunctionCallExpression expression)
    {
        Debug.Assert(parsingContext.CallLocation is not null);
        ICallLocation CallLocation = parsingContext.CallLocation!;

        FunctionCallStatementEntry NewEntry = new FunctionCallStatementEntry()
        {
            HostMethod = parsingContext.HostMethod,
            Expression = expression,
            CallLocation = CallLocation,
        };

        parsingContext.FunctionCallExpressionList.Add(NewEntry);
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
