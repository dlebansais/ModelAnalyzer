namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private bool TryParseInitializerNode(ParsingContext parsingContext, EqualsValueClauseSyntax equalsValueClause, ExpressionType variableType, out ILiteralExpression? initializerExpression)
    {
        ExpressionSyntax InitializerValue = equalsValueClause.Value;

        if (InitializerValue is LiteralExpressionSyntax LiteralExpression)
        {
            if (TryParseLiteralInitializerNode(parsingContext, LiteralExpression, variableType, out initializerExpression))
                return true;
        }
        else if (InitializerValue is ImplicitObjectCreationExpressionSyntax ImplicitObjectCreationExpression)
        {
            if (TryParseNewObjectNode(parsingContext, ImplicitObjectCreationExpression, variableType, out initializerExpression))
                return true;
        }

        LogWarning("Unsupported variable initializer.");

        Location Location = InitializerValue.GetLocation();
        parsingContext.Unsupported.AddUnsupportedExpression(Location);
        initializerExpression = null;
        return false;
    }

    private bool TryParseLiteralInitializerNode(ParsingContext parsingContext, LiteralExpressionSyntax literalExpression, ExpressionType variableType, out ILiteralExpression? initializerExpression)
    {
        Expression? ParsedExpression = TryParseLiteralValueExpression(literalExpression);

        Dictionary<ExpressionType, Func<Expression?, ILiteralExpression?>> InitializerTable = new()
        {
            { ExpressionType.Boolean, BooleanInitializer },
            { ExpressionType.Integer, IntegerInitializer },
            { ExpressionType.FloatingPoint, FloatingPointInitializer },
        };

        if (InitializerTable.ContainsKey(variableType))
        {
            initializerExpression = InitializerTable[variableType](ParsedExpression);
            if (initializerExpression is not null)
                return true;
        }
        else if (variableType.IsNullable && ParsedExpression is LiteralNullExpression NullExpression)
        {
            initializerExpression = NullExpression;
            return true;
        }

        initializerExpression = null;
        return false;
    }

    private bool TryParseNewObjectNode(ParsingContext parsingContext, ImplicitObjectCreationExpressionSyntax implicitObjectCreationExpression, ExpressionType variableType, out ILiteralExpression? initializerExpression)
    {
        if (implicitObjectCreationExpression.ArgumentList.Arguments.Count == 0 && implicitObjectCreationExpression.Initializer is null)
        {
            ExpressionType ObjectType = new ExpressionType(variableType.Name, isNullable: false);
            initializerExpression = new NewObjectExpression() { ObjectType = ObjectType };
            return true;
        }

        initializerExpression = null;
        return false;
    }

    private ILiteralExpression? BooleanInitializer(Expression? expression)
    {
        if (expression is LiteralBooleanValueExpression BooleanExpression)
            return BooleanExpression;
        else
            return null;
    }

    private ILiteralExpression? IntegerInitializer(Expression? expression)
    {
        if (expression is LiteralIntegerValueExpression IntegerExpression)
            return IntegerExpression;
        else
            return null;
    }

    private ILiteralExpression? FloatingPointInitializer(Expression? expression)
    {
        if (expression is LiteralIntegerValueExpression IntegerExpression)
            return IntegerExpression;
        else if (expression is LiteralFloatingPointValueExpression FloatingPointExpression)
            return FloatingPointExpression;
        else
            return null;
    }
}
