namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private List<Statement> ParseStatements(ParsingContext parsingContext, MethodDeclarationSyntax methodDeclaration)
    {
        List<Statement> StatementList = new();

        if (methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ArrowExpressionClause)
            StatementList = ParseMethodExpressionBody(parsingContext, ArrowExpressionClause.Expression);

        if (methodDeclaration.Body is BlockSyntax Block)
            StatementList = ParseMethodBlock(parsingContext, Block);

        return StatementList;
    }

    private List<Statement> ParseMethodExpressionBody(ParsingContext parsingContext, ExpressionSyntax expressionBody)
    {
        List<Statement> Result = new();
        LocationContext LocationContext = new(expressionBody);
        ParsingContext ExpressionBodyContext = parsingContext with { LocationContext = LocationContext };

        Expression? Expression = ParseExpression(ExpressionBodyContext, expressionBody);
        if (Expression is not null)
            Result.Add(new ReturnStatement { Expression = Expression });

        return Result;
    }

    private List<Statement> ParseMethodBlock(ParsingContext parsingContext, BlockSyntax block)
    {
        return ParseBlockStatements(parsingContext, block, isMainBlock: true);
    }

    private List<Statement> ParseBlockStatements(ParsingContext parsingContext, BlockSyntax block, bool isMainBlock)
    {
        List<Statement> StatementList = new();

        foreach (StatementSyntax Item in block.Statements)
            if (Item is not LocalDeclarationStatementSyntax)
            {
                Statement? NewStatement = ParseStatement(parsingContext, Item, isMainBlock && Item == block.Statements.Last());
                if (NewStatement is not null)
                    StatementList.Add(NewStatement);
            }

        return StatementList;
    }

    private List<Statement> ParseStatementOrBlock(ParsingContext parsingContext, StatementSyntax node)
    {
        List<Statement> StatementList = new();

        if (node is BlockSyntax Block)
            StatementList = ParseBlockStatements(parsingContext, Block, isMainBlock: false);
        else
        {
            Statement? NewStatement = ParseStatement(parsingContext, node, isLastStatement: false);
            if (NewStatement is not null)
                StatementList.Add(NewStatement);
        }

        return StatementList;
    }

    private Statement? ParseStatement(ParsingContext parsingContext, StatementSyntax statementNode, bool isLastStatement)
    {
        Statement? NewStatement = null;
        bool IsErrorReported = false;

        if (statementNode is ExpressionStatementSyntax ExpressionStatement)
            NewStatement = TryParseExpressionStatement(parsingContext, ExpressionStatement, ref IsErrorReported);
        else if (statementNode is IfStatementSyntax IfStatement)
            NewStatement = TryParseIfStatement(parsingContext, IfStatement, ref IsErrorReported);
        else if (statementNode is ReturnStatementSyntax ReturnStatement && isLastStatement)
            NewStatement = TryParseReturnStatement(parsingContext, ReturnStatement, ref IsErrorReported);
        else
            Log($"Unsupported statement type '{statementNode.GetType().Name}'.");

        if (NewStatement is null)
        {
            if (!IsErrorReported)
            {
                Location Location = statementNode.GetLocation();
                parsingContext.Unsupported.AddUnsupportedStatement(Location);
            }
        }
        else
            Log($"Statement analyzed: '{NewStatement}'.");

        return NewStatement;
    }

    private Statement? TryParseExpressionStatement(ParsingContext parsingContext, ExpressionStatementSyntax expressionStatement, ref bool isErrorReported)
    {
        ExpressionSyntax Expression = expressionStatement.Expression;

        if (Expression is AssignmentExpressionSyntax AssignmentExpression)
            return TryParseAssignmentStatement(parsingContext, AssignmentExpression, ref isErrorReported);
        else if (Expression is InvocationExpressionSyntax InvocationExpression)
            return TryParseMethodCallStatement(parsingContext, InvocationExpression, ref isErrorReported);
        else
        {
            Log("Unsupported assignment statement source.");

            return null;
        }
    }

    private Statement? TryParseAssignmentStatement(ParsingContext parsingContext, AssignmentExpressionSyntax assignmentExpression, ref bool isErrorReported)
    {
        Statement? NewStatement = null;

        if (assignmentExpression.Left is IdentifierNameSyntax IdentifierName)
        {
            string DestinationName = IdentifierName.Identifier.ValueText;
            IVariable? Destination = null;

            if (TryFindFieldByName(parsingContext, DestinationName, out IField FieldDestination))
                Destination = FieldDestination;
            else if (TryFindLocalByName(parsingContext, DestinationName, out ILocal LocalDestination))
                Destination = LocalDestination;

            if (Destination is not null)
            {
                ExpressionSyntax SourceExpression = assignmentExpression.Right;
                LocationContext LocationContext = new(SourceExpression);
                ParsingContext AssignmentParsingContext = parsingContext with { LocationContext = LocationContext, IsExpressionNested = false };

                Expression? Expression = ParseExpression(AssignmentParsingContext, SourceExpression);
                if (Expression is not null)
                {
                    if (IsSourceAndDestinationTypeCompatible(parsingContext, Destination, Expression))
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

    private bool IsSourceAndDestinationTypeCompatible(ParsingContext parsingContext, IVariable destination, Expression source)
    {
        ExpressionType DestinationType = destination.Type;
        ExpressionType SourceType = source.GetExpressionType(parsingContext);

        return IsSourceAndDestinationTypeCompatible(DestinationType, SourceType);
    }

    private bool IsSourceAndDestinationTypeCompatible(ExpressionType destinationType, ExpressionType sourceType)
    {
        if (destinationType == sourceType)
            return true;
        else if (destinationType == ExpressionType.FloatingPoint && sourceType == ExpressionType.Integer)
            return true;
        else
            return false;
    }

    private Statement? TryParseMethodCallStatement(ParsingContext parsingContext, InvocationExpressionSyntax invocationExpression, ref bool isErrorReported)
    {
        Statement? NewStatement = null;

        if (invocationExpression.Expression is IdentifierNameSyntax IdentifierName)
        {
            MethodName MethodName = new() { Text = IdentifierName.Identifier.ValueText };
            SeparatedSyntaxList<ArgumentSyntax> InvocationArgumentList = invocationExpression.ArgumentList.Arguments;
            List<Argument> ArgumentList = new();

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
                    ParsingContext MethodCallParsingContext = parsingContext with { LocationContext = LocationContext, IsExpressionNested = false };

                    Expression? Expression = ParseExpression(MethodCallParsingContext, ArgumentExpression);
                    if (Expression is not null)
                    {
                        Argument NewArgument = new() { Expression = Expression, Location = InvocationArgument.GetLocation() };
                        ArgumentList.Add(NewArgument);
                    }
                }
            }

            if (ArgumentList.Count == InvocationArgumentList.Count)
                NewStatement = new MethodCallStatement { MethodName = MethodName, NameLocation = IdentifierName.GetLocation(), ArgumentList = ArgumentList };
        }
        else
            Log("Unsupported method name.");

        return NewStatement;
    }

    private Statement? TryParseIfStatement(ParsingContext parsingContext, IfStatementSyntax ifStatement, ref bool isErrorReported)
    {
        Statement? NewStatement = null;
        ExpressionSyntax ConditionExpression = ifStatement.Condition;
        LocationContext LocationContext = new(ConditionExpression);
        ParsingContext ConditionParsingContext = parsingContext with { LocationContext = LocationContext, IsExpressionNested = false };

        Expression? Condition = ParseExpression(ConditionParsingContext, ConditionExpression);
        if (Condition is not null)
        {
            List<Statement> WhenTrueStatementList = ParseStatementOrBlock(parsingContext, ifStatement.Statement);
            List<Statement> WhenFalseStatementList;

            if (ifStatement.Else is ElseClauseSyntax ElseClause)
                WhenFalseStatementList = ParseStatementOrBlock(parsingContext, ElseClause.Statement);
            else
                WhenFalseStatementList = new();

            NewStatement = new ConditionalStatement { Condition = Condition, WhenTrueStatementList = WhenTrueStatementList, WhenFalseStatementList = WhenFalseStatementList };
        }
        else
            isErrorReported = true;

        return NewStatement;
    }

    private Statement? TryParseReturnStatement(ParsingContext parsingContext, ReturnStatementSyntax returnStatement, ref bool isErrorReported)
    {
        Statement? NewStatement = null;

        if (returnStatement.Expression is ExpressionSyntax ResultExpression)
        {
            LocationContext LocationContext = new(ResultExpression);
            ParsingContext ReturnParsingContext = parsingContext with { LocationContext = LocationContext, IsExpressionNested = false };

            Expression? ReturnExpression = ParseExpression(ReturnParsingContext, ResultExpression);

            if (ReturnExpression is not null)
            {
                LocalName ResultName = new LocalName() { Text = Ensure.ResultKeyword };
                bool IsResultInLocals = parsingContext.HostMethod!.LocalTable.ContainsItem(ResultName);
                bool IsResultReturned = ReturnExpression is VariableValueExpression VariableValue && VariableValue.VariableName.Text == ResultName.Text;

                if (IsResultReturned || !IsResultInLocals)
                {
                    NewStatement = new ReturnStatement { Expression = ReturnExpression };
                }
            }
            else
                isErrorReported = true;
        }
        else
            NewStatement = new ReturnStatement() { Expression = null };

        return NewStatement;
    }
}
