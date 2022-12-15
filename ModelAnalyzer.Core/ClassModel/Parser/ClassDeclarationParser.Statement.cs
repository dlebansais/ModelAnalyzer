namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private List<IStatement> ParseStatements(MethodDeclarationSyntax methodDeclaration, FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported)
    {
        List<IStatement> StatementList = new();

        if (methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ArrowExpressionClause)
            StatementList = ParseExpressionBody(fieldTable, parameterTable, unsupported, ArrowExpressionClause.Expression);

        if (methodDeclaration.Body is BlockSyntax Block)
            StatementList = ParseBlock(fieldTable, parameterTable, unsupported, Block, isMainBlock: true);

        return StatementList;
    }

    private List<IStatement> ParseExpressionBody(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionSyntax expressionBody)
    {
        IExpression Expression = ParseExpression(fieldTable, parameterTable, unsupported, expressionBody, isNested: false);
        return new List<IStatement>() { new ReturnStatement { Expression = Expression } };
    }

    private List<IStatement> ParseBlock(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, BlockSyntax block, bool isMainBlock)
    {
        List<IStatement> StatementList = new();

        foreach (StatementSyntax Item in block.Statements)
        {
            IStatement NewStatement = ParseStatement(fieldTable, parameterTable, unsupported, Item, isMainBlock && Item == block.Statements.Last());
            StatementList.Add(NewStatement);
        }

        return StatementList;
    }

    private List<IStatement> ParseStatementOrBlock(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, StatementSyntax node)
    {
        List<IStatement> StatementList;

        if (node is BlockSyntax Block)
            StatementList = ParseBlock(fieldTable, parameterTable, unsupported, Block, isMainBlock: false);
        else
        {
            IStatement Statement = ParseStatement(fieldTable, parameterTable, unsupported, node, isLastStatement: false);
            StatementList = new List<IStatement> { Statement };
        }

        return StatementList;
    }

    private IStatement ParseStatement(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, StatementSyntax statementNode, bool isLastStatement)
    {
        IStatement? NewStatement = null;

        if (statementNode is ExpressionStatementSyntax ExpressionStatement)
            NewStatement = TryParseAssignmentStatement(fieldTable, parameterTable, unsupported, ExpressionStatement);
        else if (statementNode is IfStatementSyntax IfStatement)
            NewStatement = TryParseIfStatement(fieldTable, parameterTable, unsupported, IfStatement);
        else if (statementNode is ReturnStatementSyntax ReturnStatement && isLastStatement)
            NewStatement = TryParseReturnStatement(fieldTable, parameterTable, unsupported, ReturnStatement);
        else
            Log($"Unsupported statement type '{statementNode.GetType().Name}'.");

        if (NewStatement is null)
        {
            Location Location = statementNode.GetLocation();
            unsupported.AddUnsupportedStatement(Location, out IUnsupportedStatement UnsupportedStatement);

            NewStatement = UnsupportedStatement;
        }
        else
            Log($"Statement analyzed: '{NewStatement}'.");

        return NewStatement;
    }

    private IStatement? TryParseAssignmentStatement(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionStatementSyntax expressionStatement)
    {
        IStatement? NewStatement = null;

        if (expressionStatement.Expression is AssignmentExpressionSyntax AssignmentExpression && AssignmentExpression.Left is IdentifierNameSyntax IdentifierName)
        {
            if (TryFindFieldByName(fieldTable, IdentifierName.Identifier.ValueText, out IField Destination))
            {
                IExpression Expression = ParseExpression(fieldTable, parameterTable, unsupported, AssignmentExpression.Right, isNested: false);
                NewStatement = new AssignmentStatement { Destination = Destination, Expression = Expression };
            }
            else
                Log($"Unsupported assignment statement destination.");
        }
        else
            Log($"Unsupported assignment statement source.");

        return NewStatement;
    }

    private IStatement? TryParseIfStatement(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, IfStatementSyntax ifStatement)
    {
        IExpression Condition = ParseExpression(fieldTable, parameterTable, unsupported, ifStatement.Condition, isNested: false);
        List<IStatement> WhenTrueStatementList = ParseStatementOrBlock(fieldTable, parameterTable, unsupported, ifStatement.Statement);
        List<IStatement> WhenFalseStatementList;

        if (ifStatement.Else is ElseClauseSyntax ElseClause)
            WhenFalseStatementList = ParseStatementOrBlock(fieldTable, parameterTable, unsupported, ElseClause.Statement);
        else
            WhenFalseStatementList = new();

        return new ConditionalStatement { Condition = Condition, WhenTrueStatementList = WhenTrueStatementList, WhenFalseStatementList = WhenFalseStatementList };
    }

    private IStatement? TryParseReturnStatement(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ReturnStatementSyntax returnStatement)
    {
        IExpression? ReturnExpression;

        if (returnStatement.Expression is not null)
            ReturnExpression = ParseExpression(fieldTable, parameterTable, unsupported, returnStatement.Expression, isNested: false);
        else
            ReturnExpression = null;

        return new ReturnStatement { Expression = ReturnExpression };
    }
}
