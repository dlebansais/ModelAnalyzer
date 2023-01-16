namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
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
        List<Statement> StatementList = new();
        LocationContext LocationContext = new(expressionBody);
        ParsingContext ExpressionBodyContext = parsingContext with { LocationContext = LocationContext, CallLocation = new CallExpressionBodyLocation() };

        Expression? Expression = ParseExpression(ExpressionBodyContext, expressionBody);
        if (Expression is not null)
            StatementList.Add(new ReturnStatement { Expression = Expression });

        return StatementList;
    }

    private List<Statement> ParseMethodBlock(ParsingContext parsingContext, BlockSyntax block)
    {
        return ParseBlockStatements(parsingContext, block, isMainBlock: true);
    }

    private List<Statement> ParseBlockStatements(ParsingContext parsingContext, BlockSyntax block, bool isMainBlock)
    {
        List<Statement> StatementList = new();

        for (int StatementIndex = 0; StatementIndex < block.Statements.Count; StatementIndex++)
        {
            StatementSyntax Item = block.Statements[StatementIndex];

            if (Item is not LocalDeclarationStatementSyntax)
            {
                CallStatementLocation CallLocation = new() { ParentStatementList = StatementList, StatementIndex = StatementIndex };
                ParsingContext BlockContext = parsingContext with { CallLocation = CallLocation };

                Statement? NewStatement = ParseStatement(BlockContext, Item, isMainBlock && Item == block.Statements.Last());
                if (NewStatement is not null)
                    StatementList.Add(NewStatement);
            }
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
            CallStatementLocation CallLocation = new() { ParentStatementList = StatementList, StatementIndex = 0 };
            ParsingContext SingleStatementContext = parsingContext with { CallLocation = CallLocation };

            Statement? NewStatement = ParseStatement(SingleStatementContext, node, isLastStatement: false);
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
        AssignmentStatement? NewStatement = null;

        if (assignmentExpression.Left is IdentifierNameSyntax IdentifierName)
        {
            string DestinationName = IdentifierName.Identifier.ValueText;
            IVariable? Destination = null;

            if (TryFindPropertyByName(parsingContext, DestinationName, out IProperty PropertyDestination))
                Destination = PropertyDestination;
            else if (TryFindFieldByName(parsingContext, DestinationName, out IField FieldDestination))
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
        else if (destinationType.IsNullable)
        {
            if (sourceType == ExpressionType.Null)
                return true;
            else if (destinationType.Name == sourceType.Name)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    private Statement? TryParseMethodCallStatement(ParsingContext parsingContext, InvocationExpressionSyntax invocationExpression, ref bool isErrorReported)
    {
        MethodCallStatement? NewStatement = null;

        if (invocationExpression.Expression is IdentifierNameSyntax IdentifierName)
        {
            MethodName MethodName = new() { Text = IdentifierName.Identifier.ValueText };
            ArgumentListSyntax InvocationArgumentList = invocationExpression.ArgumentList;
            List<Argument> ArgumentList = TryParseArgumentList(parsingContext, InvocationArgumentList, ref isErrorReported);

            if (ArgumentList.Count == InvocationArgumentList.Arguments.Count)
            {
                Debug.Assert(parsingContext.HostMethod is not null);
                Debug.Assert(parsingContext.CallLocation is not null);
                ICallLocation CallLocation = parsingContext.CallLocation!;

                NewStatement = new MethodCallStatement { MethodName = MethodName, NameLocation = IdentifierName.GetLocation(), ArgumentList = ArgumentList };
                parsingContext.MethodCallStatementList.Add(new MethodCallStatementEntry() { HostMethod = parsingContext.HostMethod!, Statement = NewStatement, CallLocation = CallLocation });
            }
        }
        else
            Log("Unsupported method name.");

        return NewStatement;
    }

    private Statement? TryParseIfStatement(ParsingContext parsingContext, IfStatementSyntax ifStatement, ref bool isErrorReported)
    {
        ConditionalStatement? NewStatement = null;
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
        Debug.Assert(parsingContext.HostMethod is not null);

        Method HostMethod = parsingContext.HostMethod!;
        ReturnStatement? NewStatement = null;

        if (returnStatement.Expression is ExpressionSyntax ResultExpression)
        {
            if (HostMethod.ReturnType != ExpressionType.Void)
            {
                LocationContext LocationContext = new(ResultExpression);
                ParsingContext ReturnParsingContext = parsingContext with { LocationContext = LocationContext, IsExpressionNested = false };

                Expression? ReturnExpression = ParseExpression(ReturnParsingContext, ResultExpression);

                if (ReturnExpression is not null)
                {
                    LocalName ResultName = new LocalName() { Text = Ensure.ResultKeyword };
                    bool IsResultInLocals = HostMethod.LocalTable.ContainsItem(ResultName);
                    bool IsResultReturned = ReturnExpression is VariableValueExpression VariableValue && VariableValue.VariablePath.Count == 1 && VariableValue.VariablePath[0].Name.Text == ResultName.Text;

                    if (IsResultReturned || !IsResultInLocals)
                        NewStatement = new ReturnStatement { Expression = ReturnExpression };
                }
                else
                    isErrorReported = true;
            }
        }
        else
            NewStatement = new ReturnStatement() { Expression = null };

        return NewStatement;
    }
}
