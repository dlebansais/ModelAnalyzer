namespace DemoAnalyzer;

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
        else if (methodDeclaration.Body is BlockSyntax Block)
            StatementList = ParseBlock(fieldTable, parameterTable, unsupported, Block, isMainBlock: true);

        return StatementList;
    }

    private List<IStatement> ParseExpressionBody(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionSyntax expressionBody)
    {
        IExpression Expression = ParseExpression(fieldTable, parameterTable, unsupported, expressionBody);
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

        if (statementNode.AttributeLists.Count == 0)
        {
            if (statementNode is ExpressionStatementSyntax ExpressionStatement)
                NewStatement = ParseAssignmentStatement(fieldTable, parameterTable, unsupported, ExpressionStatement);
            else if (statementNode is IfStatementSyntax IfStatement)
                NewStatement = ParseIfStatement(fieldTable, parameterTable, unsupported, IfStatement);
            else if (statementNode is ReturnStatementSyntax ReturnStatement && isLastStatement)
                NewStatement = ParseReturnStatement(fieldTable, parameterTable, unsupported, ReturnStatement);
            else
                Log($"Unsupported statement type '{statementNode.GetType().Name}'.");
        }

        if (NewStatement is null)
        {
            Location Location = statementNode.GetLocation();
            unsupported.AddUnsupportedStatement(Location, out IUnsupportedStatement UnsupportedStatement);

            NewStatement = UnsupportedStatement;
        }

        return NewStatement;
    }

    private IStatement ParseAssignmentStatement(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionStatementSyntax node)
    {
        IStatement NewStatement;

        if (node.Expression is AssignmentExpressionSyntax AssignmentExpression &&
            AssignmentExpression.Left is IdentifierNameSyntax IdentifierName &&
            TryFindFieldByName(fieldTable, IdentifierName.Identifier.ValueText, out IField Destination))
        {
            IExpression Expression = ParseExpression(fieldTable, parameterTable, unsupported, AssignmentExpression.Right);
            NewStatement = new AssignmentStatement { Destination = Destination, Expression = Expression };
        }
        else
        {
            Log($"Unsupported assignment statement syntax.");

            Location Location = node.GetLocation();
            unsupported.AddUnsupportedStatement(Location, out IUnsupportedStatement UnsupportedStatement);

            NewStatement = UnsupportedStatement;
        }

        return NewStatement;
    }

    private IStatement ParseIfStatement(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, IfStatementSyntax node)
    {
        IExpression Condition = ParseExpression(fieldTable, parameterTable, unsupported, node.Condition);
        List<IStatement> WhenTrueStatementList = ParseStatementOrBlock(fieldTable, parameterTable, unsupported, node.Statement);
        List<IStatement> WhenFalseStatementList;

        if (node.Else is ElseClauseSyntax ElseClause)
            WhenFalseStatementList = ParseStatementOrBlock(fieldTable, parameterTable, unsupported, ElseClause.Statement);
        else
            WhenFalseStatementList = new();

        return new ConditionalStatement { Condition = Condition, WhenTrueStatementList = WhenTrueStatementList, WhenFalseStatementList = WhenFalseStatementList };
    }

    private IStatement ParseReturnStatement(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ReturnStatementSyntax node)
    {
        IExpression? ReturnExpression;

        if (node.Expression is not null)
            ReturnExpression = ParseExpression(fieldTable, parameterTable, unsupported, node.Expression);
        else
            ReturnExpression = null;

        return new ReturnStatement { Expression = ReturnExpression };
    }
}
