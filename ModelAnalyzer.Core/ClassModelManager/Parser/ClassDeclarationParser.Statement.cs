﻿namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private List<Statement> ParseStatements(MethodDeclarationSyntax methodDeclaration, ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Unsupported unsupported)
    {
        List<Statement> StatementList = new();

        if (methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ArrowExpressionClause)
            StatementList = ParseExpressionBody(fieldTable, parameterTable, unsupported, ArrowExpressionClause.Expression);

        if (methodDeclaration.Body is BlockSyntax Block)
            StatementList = ParseBlock(fieldTable, parameterTable, unsupported, Block, isMainBlock: true);

        return StatementList;
    }

    private List<Statement> ParseExpressionBody(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Unsupported unsupported, ExpressionSyntax expressionBody)
    {
        List<Statement> Result = new();
        LocationContext LocationContext = new(expressionBody);

        Expression? Expression = ParseExpression(fieldTable, parameterTable, resultField: null, unsupported, LocationContext, expressionBody, isNested: false);
        if (Expression is not null)
            Result.Add(new ReturnStatement { Expression = Expression });

        return Result;
    }

    private List<Statement> ParseBlock(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Unsupported unsupported, BlockSyntax block, bool isMainBlock)
    {
        List<Statement> StatementList = new();

        foreach (StatementSyntax Item in block.Statements)
        {
            Statement? NewStatement = ParseStatement(fieldTable, parameterTable, unsupported, Item, isMainBlock && Item == block.Statements.Last());
            if (NewStatement is not null)
                StatementList.Add(NewStatement);
        }

        return StatementList;
    }

    private List<Statement> ParseStatementOrBlock(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Unsupported unsupported, StatementSyntax node)
    {
        List<Statement> StatementList = new();

        if (node is BlockSyntax Block)
            StatementList = ParseBlock(fieldTable, parameterTable, unsupported, Block, isMainBlock: false);
        else
        {
            Statement? NewStatement = ParseStatement(fieldTable, parameterTable, unsupported, node, isLastStatement: false);
            if (NewStatement is not null)
                StatementList.Add(NewStatement);
        }

        return StatementList;
    }

    private Statement? ParseStatement(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Unsupported unsupported, StatementSyntax statementNode, bool isLastStatement)
    {
        Statement? NewStatement = null;
        bool IsErrorReported = false;

        if (statementNode is ExpressionStatementSyntax ExpressionStatement)
            NewStatement = TryParseExpressionStatement(fieldTable, parameterTable, unsupported, ExpressionStatement, ref IsErrorReported);
        else if (statementNode is IfStatementSyntax IfStatement)
            NewStatement = TryParseIfStatement(fieldTable, parameterTable, unsupported, IfStatement, ref IsErrorReported);
        else if (statementNode is ReturnStatementSyntax ReturnStatement && isLastStatement)
            NewStatement = TryParseReturnStatement(fieldTable, parameterTable, unsupported, ReturnStatement, ref IsErrorReported);
        else
            Log($"Unsupported statement type '{statementNode.GetType().Name}'.");

        if (NewStatement is null)
        {
            if (!IsErrorReported)
            {
                Location Location = statementNode.GetLocation();
                unsupported.AddUnsupportedStatement(Location);
            }
        }
        else
            Log($"Statement analyzed: '{NewStatement}'.");

        return NewStatement;
    }

    private Statement? TryParseExpressionStatement(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Unsupported unsupported, ExpressionStatementSyntax expressionStatement, ref bool isErrorReported)
    {
        ExpressionSyntax Expression = expressionStatement.Expression;

        if (Expression is AssignmentExpressionSyntax AssignmentExpression)
            return TryParseAssignmentStatement(fieldTable, parameterTable, unsupported, AssignmentExpression, ref isErrorReported);
        else if (Expression is InvocationExpressionSyntax InvocationExpression)
            return TryParseMethodCallStatement(fieldTable, parameterTable, unsupported, InvocationExpression, ref isErrorReported);
        else
        {
            Log("Unsupported assignment statement source.");

            return null;
        }
    }

    private Statement? TryParseAssignmentStatement(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Unsupported unsupported, AssignmentExpressionSyntax assignmentExpression, ref bool isErrorReported)
    {
        Statement? NewStatement = null;

        if (assignmentExpression.Left is IdentifierNameSyntax IdentifierName)
        {
            if (TryFindFieldByName(fieldTable, IdentifierName.Identifier.ValueText, out Field Destination))
            {
                ExpressionSyntax SourceExpression = assignmentExpression.Right;
                LocationContext LocationContext = new(SourceExpression);

                Expression? Expression = ParseExpression(fieldTable, parameterTable, resultField: null, unsupported, LocationContext, SourceExpression, isNested: false);
                if (Expression is not null)
                {
                    if (IsSourceAndDestinationTypeCompatible(fieldTable, parameterTable, resultField: null, Destination, Expression))
                        NewStatement = new AssignmentStatement { DestinationName = Destination.Name, Expression = Expression };
                    else
                        Log("Source cannot be assigned to destination.");
                }
                else
                    isErrorReported = true;
            }
            else
                Log("Unknown assignment statement destination.");
        }
        else
            Log("Unsupported assignment statement destination.");

        return NewStatement;
    }

    private bool IsSourceAndDestinationTypeCompatible(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Field? resultField, Field destination, Expression source)
    {
        if (destination.Type == source.GetExpressionType(fieldTable, parameterTable, resultField))
            return true;
        else if (destination.Type == ExpressionType.FloatingPoint && source.GetExpressionType(fieldTable, parameterTable, resultField) == ExpressionType.Integer)
            return true;
        else
            return false;
    }

    private Statement? TryParseMethodCallStatement(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Unsupported unsupported, InvocationExpressionSyntax invocationExpression, ref bool isErrorReported)
    {
        Statement? NewStatement = null;

        if (invocationExpression.Expression is IdentifierNameSyntax IdentifierName)
        {
            MethodName MethodName = new() { Text = IdentifierName.Identifier.ValueText };
            SeparatedSyntaxList<ArgumentSyntax> InvocationArgumentList = invocationExpression.ArgumentList.Arguments;
            List<IExpression> ArgumentList = new();

            foreach (ArgumentSyntax InvocationArgument in InvocationArgumentList)
            {
                if (InvocationArgument.NameColon is not null)
                    Log("Named argument not supported.");
                else if (!InvocationArgument.RefKindKeyword.IsKind(SyntaxKind.None))
                    Log("ref, out or in arguments not supported.");
                else
                {
                    ExpressionSyntax ArgumentExpression = InvocationArgument.Expression;
                    LocationContext LocationContext = new(ArgumentExpression);

                    Expression? Argument = ParseExpression(fieldTable, parameterTable, resultField: null, unsupported, LocationContext, ArgumentExpression, isNested: false);
                    if (Argument is not null)
                        ArgumentList.Add(Argument);
                }
            }

            if (ArgumentList.Count == InvocationArgumentList.Count)
                NewStatement = new MethodCallStatement { MethodName = MethodName, ArgumentList = ArgumentList };
        }
        else
            Log("Unsupported method name.");

        return NewStatement;
    }

    private Statement? TryParseIfStatement(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Unsupported unsupported, IfStatementSyntax ifStatement, ref bool isErrorReported)
    {
        Statement? NewStatement = null;
        ExpressionSyntax ConditionExpression = ifStatement.Condition;
        LocationContext LocationContext = new(ConditionExpression);

        Expression? Condition = ParseExpression(fieldTable, parameterTable, resultField: null, unsupported, LocationContext, ConditionExpression, isNested: false);
        if (Condition is not null)
        {
            List<Statement> WhenTrueStatementList = ParseStatementOrBlock(fieldTable, parameterTable, unsupported, ifStatement.Statement);
            List<Statement> WhenFalseStatementList;

            if (ifStatement.Else is ElseClauseSyntax ElseClause)
                WhenFalseStatementList = ParseStatementOrBlock(fieldTable, parameterTable, unsupported, ElseClause.Statement);
            else
                WhenFalseStatementList = new();

            NewStatement = new ConditionalStatement { Condition = Condition, WhenTrueStatementList = WhenTrueStatementList, WhenFalseStatementList = WhenFalseStatementList };
        }
        else
            isErrorReported = true;

        return NewStatement;
    }

    private Statement? TryParseReturnStatement(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Unsupported unsupported, ReturnStatementSyntax returnStatement, ref bool isErrorReported)
    {
        Statement? NewStatement = null;

        if (returnStatement.Expression is ExpressionSyntax ResultExpression)
        {
            LocationContext LocationContext = new(ResultExpression);
            Expression? ReturnExpression = ParseExpression(fieldTable, parameterTable, resultField: null, unsupported, LocationContext, ResultExpression, isNested: false);

            if (ReturnExpression is not null)
                NewStatement = new ReturnStatement { Expression = ReturnExpression };
            else
                isErrorReported = true;
        }
        else
            NewStatement = new ReturnStatement() { Expression = null };

        return NewStatement;
    }
}
