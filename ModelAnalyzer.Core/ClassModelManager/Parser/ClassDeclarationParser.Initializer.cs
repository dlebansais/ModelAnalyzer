namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Expression? ParsedExpression = TryParseLiteralValueExpression(LiteralExpression);

            Dictionary<ExpressionType, Func<Expression?, ILiteralExpression?>> InitializerTable = new()
            {
                { ExpressionType.Boolean, BooleanInitializer },
                { ExpressionType.Integer, IntegerInitializer },
                { ExpressionType.FloatingPoint, FloatingPointInitializer },
            };

            Debug.Assert(InitializerTable.ContainsKey(variableType));

            initializerExpression = InitializerTable[variableType](ParsedExpression);
            if (initializerExpression is not null)
                return true;
        }

        LogWarning("Unsupported variable initializer.");

        Location Location = InitializerValue.GetLocation();
        parsingContext.Unsupported.AddUnsupportedExpression(Location);
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
