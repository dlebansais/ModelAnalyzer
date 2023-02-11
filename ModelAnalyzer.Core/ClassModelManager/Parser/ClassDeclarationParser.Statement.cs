namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private BlockScope ParseStatements(ParsingContext parsingContext, MethodDeclarationSyntax methodDeclaration)
    {
        BlockScope NewBlock = null!;

        if (methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ArrowExpressionClause)
            NewBlock = ParseMethodExpressionBody(parsingContext, ArrowExpressionClause.Expression);

        if (methodDeclaration.Body is BlockSyntax Block)
            NewBlock = ParseMethodBlock(parsingContext, Block);

        return NewBlock;
    }

    private BlockScope ParseMethodExpressionBody(ParsingContext parsingContext, ExpressionSyntax expressionBody)
    {
        BlockScope NewBlock = new() { LocalTable = new LocalTable().AsReadOnly(), StatementList = new List<Statement>() };
        LocationContext LocationContext = new(expressionBody);
        ParsingContext ExpressionBodyContext = parsingContext with { LocationContext = LocationContext, CallLocation = new CallExpressionBodyLocation() };

        Expression? Expression = ParseExpression(ExpressionBodyContext, expressionBody);
        if (Expression is not null)
        {
            ReturnStatement NewStatement = new() { Expression = Expression };
            NewBlock.StatementList.Add(NewStatement);

            Debug.Assert(NewStatement.LocationId != LocationId.None);
        }

        return NewBlock;
    }

    private BlockScope ParseMethodBlock(ParsingContext parsingContext, BlockSyntax block)
    {
        return ParseBlockStatements(parsingContext, block, isMainBlock: true);
    }

    private BlockScope ParseBlockStatements(ParsingContext parsingContext, BlockSyntax block, bool isMainBlock)
    {
        Debug.Assert(parsingContext.HostBlock is not null);
        BlockScope HostBlock = parsingContext.HostBlock!;

        BlockScope NewBlock = new() { LocalTable = HostBlock.LocalTable, StatementList = new List<Statement>() };

        for (int StatementIndex = 0; StatementIndex < block.Statements.Count; StatementIndex++)
        {
            StatementSyntax Item = block.Statements[StatementIndex];

            if (Item is not LocalDeclarationStatementSyntax)
            {
                CallStatementLocation CallLocation = new() { ParentBlock = NewBlock, StatementIndex = StatementIndex };
                ParsingContext BlockContext = parsingContext with { CallLocation = CallLocation };

                Statement? NewStatement = ParseStatement(BlockContext, Item, isMainBlock && Item == block.Statements.Last());
                if (NewStatement is not null)
                    NewBlock.StatementList.Add(NewStatement);
            }
        }

        return NewBlock;
    }

    private BlockScope ParseStatementOrBlock(ParsingContext parsingContext, StatementSyntax node)
    {
        BlockScope NewBlock;

        if (node is BlockSyntax Block)
            NewBlock = ParseBlockStatements(parsingContext, Block, isMainBlock: false);
        else
        {
            Debug.Assert(parsingContext.HostBlock is not null);
            BlockScope HostBlock = parsingContext.HostBlock!;

            NewBlock = new() { LocalTable = HostBlock.LocalTable, StatementList = new List<Statement>() };

            CallStatementLocation CallLocation = new() { ParentBlock = NewBlock, StatementIndex = 0 };
            ParsingContext SingleStatementContext = parsingContext with { CallLocation = CallLocation };

            Statement? NewStatement = ParseStatement(SingleStatementContext, node, isLastStatement: false);
            if (NewStatement is not null)
                NewBlock.StatementList.Add(NewStatement);
        }

        return NewBlock;
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
        else if (statementNode is ForStatementSyntax ForStatement)
            NewStatement = TryParseForStatement(parsingContext, ForStatement, ref IsErrorReported);
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
            return TryParseAssignmentToIdentifierStatement(parsingContext, IdentifierName, assignmentExpression.Right, ref isErrorReported);
        else if (assignmentExpression.Left is ElementAccessExpressionSyntax ElementAccessExpression)
            return TryParseAssignmentToElementStatement(parsingContext, ElementAccessExpression, assignmentExpression.Right, ref isErrorReported);
        else
            Log("Unsupported assignment statement destination.");

        return NewStatement;
    }

    private Statement? TryParseAssignmentToIdentifierStatement(ParsingContext parsingContext, IdentifierNameSyntax identifierName, ExpressionSyntax rightExpression, ref bool isErrorReported)
    {
        AssignmentStatement? NewStatement = null;

        if (TryParseAssignmentDestinationIdentifier(parsingContext, identifierName, out IVariable Destination))
        {
            ExpressionSyntax SourceExpression = rightExpression;
            LocationContext LocationContext = new(SourceExpression);
            ParsingContext AssignmentParsingContext = parsingContext with { LocationContext = LocationContext, IsExpressionNested = false };

            Expression? Expression = ParseExpression(AssignmentParsingContext, SourceExpression);
            if (Expression is not null)
            {
                if (IsSourceAndDestinationTypeCompatible(Destination, Expression))
                {
                    NewStatement = new AssignmentStatement
                    {
                        DestinationName = Destination.Name,
                        DestinationIndex = null,
                        Expression = Expression,
                    };

                    Debug.Assert(NewStatement.LocationId != LocationId.None);
                }
                else
                    Log("Source cannot be assigned to destination.");
            }
            else
                isErrorReported = true;
        }
        else
            Log("Unknown assignment statement destination.");

        return NewStatement;
    }

    private Statement? TryParseAssignmentToElementStatement(ParsingContext parsingContext, ElementAccessExpressionSyntax elementAccessExpression, ExpressionSyntax rightExpression, ref bool isErrorReported)
    {
        AssignmentStatement? NewStatement = null;

        if (elementAccessExpression.Expression is IdentifierNameSyntax IdentifierName)
        {
            if (TryParseAssignmentDestinationIdentifier(parsingContext, IdentifierName, out IVariable Destination))
            {
                if (TryParseElementIndex(parsingContext, elementAccessExpression.ArgumentList.Arguments, out Expression ElementIndex, ref isErrorReported))
                {
                    ExpressionSyntax SourceExpression = rightExpression;
                    LocationContext LocationContext = new(SourceExpression);
                    ParsingContext AssignmentParsingContext = parsingContext with { LocationContext = LocationContext, IsExpressionNested = false };

                    Expression? Expression = ParseExpression(AssignmentParsingContext, SourceExpression);
                    if (Expression is not null)
                    {
                        if (IsSourceAndDestinationArrayTypeCompatible(Destination, Expression))
                        {
                            NewStatement = new AssignmentStatement
                            {
                                DestinationName = Destination.Name,
                                DestinationIndex = ElementIndex,
                                Expression = Expression,
                            };

                            Debug.Assert(NewStatement.LocationId != LocationId.None);
                        }
                        else
                            Log("Source cannot be assigned to destination.");
                    }
                    else
                        isErrorReported = true;
                }
                else
                    Log("Unable to parse destination element index.");
            }
            else
                Log("Unknown destination variable.");
        }
        else
            Log("Unknown assignment statement destination.");

        return NewStatement;
    }

    private bool TryParseAssignmentDestinationIdentifier(ParsingContext parsingContext, IdentifierNameSyntax identifierName, out IVariable destination)
    {
        string DestinationName = identifierName.Identifier.ValueText;

        if (TryFindPropertyByName(parsingContext, DestinationName, out IProperty PropertyDestination))
        {
            destination = PropertyDestination;
            return true;
        }
        else if (TryFindFieldByName(parsingContext, DestinationName, out IField FieldDestination))
        {
            destination = FieldDestination;
            return true;
        }
        else if (TryFindLocalByName(parsingContext, DestinationName, out ILocal LocalDestination))
        {
            destination = LocalDestination;
            return true;
        }

        destination = null!;
        return false;
    }

    private bool IsSourceAndDestinationTypeCompatible(IVariable destination, Expression source)
    {
        ExpressionType DestinationType = destination.Type;
        ExpressionType SourceType = source.GetExpressionType();

        return IsSourceAndDestinationTypeCompatible(DestinationType, SourceType);
    }

    private bool IsSourceAndDestinationArrayTypeCompatible(IVariable destination, Expression source)
    {
        ExpressionType DestinationType = destination.Type;
        ExpressionType SourceType = source.GetExpressionType();

        if (DestinationType.IsArray)
            return IsSourceAndDestinationTypeCompatible(DestinationType.ToElementType(), SourceType);
        else
            return false;
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
            else if (destinationType.TypeName == sourceType.TypeName)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    private Statement? TryParseMethodCallStatement(ParsingContext parsingContext, InvocationExpressionSyntax invocationExpression, ref bool isErrorReported)
    {
        Statement? NewStatement = null;

        ArgumentListSyntax InvocationArgumentList = invocationExpression.ArgumentList;
        List<Argument> ArgumentList = TryParseArgumentList(parsingContext, InvocationArgumentList, ref isErrorReported);

        if (ArgumentList.Count == InvocationArgumentList.Arguments.Count)
        {
            if (invocationExpression.Expression is IdentifierNameSyntax IdentifierName)
                NewStatement = TryParsePrivateMethodCallStatement(parsingContext, IdentifierName, ArgumentList, ref isErrorReported);
            else if (invocationExpression.Expression is MemberAccessExpressionSyntax MemberAccessExpression)
                NewStatement = TryParsePublicMethodCallStatement(parsingContext, MemberAccessExpression, ArgumentList, ref isErrorReported);
            else
                Log("Unsupported method name.");
        }

        return NewStatement;
    }

    private PrivateMethodCallStatement? TryParsePrivateMethodCallStatement(ParsingContext parsingContext, IdentifierNameSyntax identifierName, List<Argument> argumentList, ref bool isErrorReported)
    {
        MethodName MethodName = new() { Text = identifierName.Identifier.ValueText };
        ClassName ClassName = parsingContext.ClassName;
        Dictionary<ClassName, IClassModel> Phase1ClassModelTable = parsingContext.SemanticModel.Phase1ClassModelTable;

        Debug.Assert(Phase1ClassModelTable.ContainsKey(ClassName));
        ClassModel ClassModel = (ClassModel)Phase1ClassModelTable[ClassName];
        bool IsStatic = false;

        foreach (KeyValuePair<MethodName, Method> Entry in ClassModel.MethodTable)
            if (Entry.Key.Text == MethodName.Text)
            {
                Method CalledMethod = Entry.Value;
                IsStatic = CalledMethod.IsStatic;
                break;
            }

        PrivateMethodCallStatement NewStatement = new()
        {
            ClassName = IsStatic ? ClassName : ClassName.Empty,
            MethodName = MethodName,
            NameLocation = identifierName.GetLocation(),
            ArgumentList = argumentList,
            CallerClassName = parsingContext.ClassName,
        };

        AddMethodCallEntry(parsingContext, NewStatement);

        Debug.Assert(NewStatement.LocationId != LocationId.None);

        return NewStatement;
    }

    private PublicMethodCallStatement? TryParsePublicMethodCallStatement(ParsingContext parsingContext, MemberAccessExpressionSyntax memberAccessExpression, List<Argument> argumentList, ref bool isErrorReported)
    {
        PublicMethodCallStatement? NewStatement = null;
        string LastName;
        Location PathLocation;

        if (TryParsePropertyPath(parsingContext, memberAccessExpression, out List<IVariable> VariablePath, out LastName, out PathLocation))
        {
            if (TryParseLastNameAsMethod(parsingContext, VariablePath, LastName, out ClassName CalledClassName, out Method CalledMethod))
            {
                NewStatement = new PublicMethodCallStatement
                {
                    ClassName = CalledClassName,
                    VariablePath = VariablePath,
                    MethodName = CalledMethod.Name,
                    NameLocation = PathLocation,
                    ArgumentList = argumentList,
                    CallerClassName = parsingContext.ClassName,
                };

                AddMethodCallEntry(parsingContext, NewStatement);

                Debug.Assert(NewStatement.LocationId != LocationId.None);
            }
        }
        else if (TryParseTypeName(parsingContext, memberAccessExpression, out ClassModel ClassModel, out LastName, out PathLocation))
        {
            if (TryParseLastNameAsMethod(parsingContext, ClassModel, LastName, out Method CalledMethod))
            {
                NewStatement = new PublicMethodCallStatement
                {
                    ClassName = ClassModel.ClassName,
                    VariablePath = new List<IVariable>(),
                    MethodName = CalledMethod.Name,
                    NameLocation = PathLocation,
                    ArgumentList = argumentList,
                    CallerClassName = parsingContext.ClassName,
                };

                AddMethodCallEntry(parsingContext, NewStatement);

                Debug.Assert(NewStatement.LocationId != LocationId.None);
            }
        }

        return NewStatement;
    }

    private void AddMethodCallEntry(ParsingContext parsingContext, IMethodCallStatement statement)
    {
        Debug.Assert(parsingContext.HostMethod is not null);
        Debug.Assert(parsingContext.CallLocation is not null);
        ICallLocation CallLocation = parsingContext.CallLocation!;

        MethodCallStatementEntry NewEntry = new()
        {
            HostMethod = parsingContext.HostMethod!,
            ParentBlock = parsingContext.HostBlock!,
            Statement = statement,
            CallLocation = CallLocation,
        };

        parsingContext.MethodCallStatementList.Add(NewEntry);
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
            BlockScope WhenTrueBlock = ParseStatementOrBlock(parsingContext, ifStatement.Statement);
            BlockScope WhenFalseBlock;

            if (ifStatement.Else is ElseClauseSyntax ElseClause)
                WhenFalseBlock = ParseStatementOrBlock(parsingContext, ElseClause.Statement);
            else
                WhenFalseBlock = new() { LocalTable = parsingContext.HostBlock!.LocalTable, StatementList = new List<Statement>() };

            NewStatement = new ConditionalStatement { Condition = Condition, WhenTrueBlock = WhenTrueBlock, WhenFalseBlock = WhenFalseBlock };

            Debug.Assert(NewStatement.LocationId != LocationId.None);
        }
        else
            isErrorReported = true;

        return NewStatement;
    }

    private Statement? TryParseReturnStatement(ParsingContext parsingContext, ReturnStatementSyntax returnStatement, ref bool isErrorReported)
    {
        Debug.Assert(parsingContext.HostMethod is not null);
        Method HostMethod = parsingContext.HostMethod!;
        Debug.Assert(parsingContext.HostBlock is not null);
        BlockScope HostBlock = parsingContext.HostBlock!;
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
                    bool IsResultInLocals = HostBlock.LocalTable.ContainsItem(ResultName);
                    bool IsResultReturned = ReturnExpression is VariableValueExpression VariableValue && VariableValue.VariablePath.Count == 1 && VariableValue.VariablePath[0].Name.Text == ResultName.Text;

                    if (IsResultReturned || !IsResultInLocals)
                    {
                        NewStatement = new ReturnStatement { Expression = ReturnExpression };

                        Debug.Assert(NewStatement.LocationId != LocationId.None);
                    }
                }
                else
                    isErrorReported = true;
            }
        }
        else
        {
            NewStatement = new ReturnStatement() { Expression = null };

            Debug.Assert(NewStatement.LocationId != LocationId.None);
        }

        return NewStatement;
    }

    private Statement? TryParseForStatement(ParsingContext parsingContext, ForStatementSyntax forStatement, ref bool isErrorReported)
    {
        ForLoopStatement? NewStatement = null;

        if (TryParseForStatementInit(parsingContext, forStatement, out Local LocalIndex))
        {
            if (forStatement.Condition is not null)
            {
                BlockScope HostBlock = parsingContext.HostBlock!;

                LocalTable ForLoopLocalTable = new();
                foreach (KeyValuePair<LocalName, Local> Entry in HostBlock.LocalTable)
                    ForLoopLocalTable.AddItem(Entry.Value);
                ForLoopLocalTable.AddItem(LocalIndex);

                BlockScope ForLoopBlock = new() { LocalTable = ForLoopLocalTable.AsReadOnly(), StatementList = new List<Statement>() };

                LocationContext LocationContext = new(forStatement.Condition);
                ParsingContext ForLoopParsingContext = parsingContext with { LocationContext = LocationContext, HostBlock = ForLoopBlock, IsExpressionNested = false };

                Expression? ContinueCondition = ParseExpression(ForLoopParsingContext, forStatement.Condition);
                if (ContinueCondition is not null)
                {
                    if (ContinueCondition.GetExpressionType() == ExpressionType.Boolean)
                    {
                        if (TryParseForStatementIncrementor(ForLoopParsingContext, forStatement, LocalIndex, ref isErrorReported))
                        {
                            ForLoopBlock = ParseStatementOrBlock(ForLoopParsingContext, forStatement.Statement);

                            NewStatement = new ForLoopStatement
                            {
                                LocalIndex = LocalIndex,
                                ContinueCondition = ContinueCondition,
                                Block = ForLoopBlock,
                            };

                            Debug.Assert(NewStatement.LocationId != LocationId.None);
                        }
                    }
                }
                else
                    isErrorReported = true;
            }
        }

        return NewStatement;
    }

    private bool TryParseForStatementInit(ParsingContext parsingContext, ForStatementSyntax forStatement, out Local localIndex)
    {
        if (forStatement.Declaration is VariableDeclarationSyntax Declaration)
        {
            // TODO: special check of index
            LocalTable ForLocalTable = new LocalTable();
            AddLocal(parsingContext, ForLocalTable, Declaration, isLocalSupported: true);

            if (ForLocalTable.List.Count == 1)
            {
                KeyValuePair<LocalName, Local> IndexEntry = ForLocalTable.List[0];
                Local LocalIndex = IndexEntry.Value;

                if (LocalIndex.Type == ExpressionType.Integer)
                {
                    localIndex = LocalIndex;
                    return true;
                }
            }
        }

        localIndex = null!;
        return false;
    }

    private bool TryParseForStatementIncrementor(ParsingContext parsingContext, ForStatementSyntax forStatement, Local localIndex, ref bool isErrorReported)
    {
        SeparatedSyntaxList<ExpressionSyntax> Incrementors = forStatement.Incrementors;

        if (Incrementors.Count == 1)
        {
            ExpressionSyntax Incrementor = Incrementors[0];
            if (Incrementor is PostfixUnaryExpressionSyntax PostfixUnaryExpression)
            {
                if (PostfixUnaryExpression.OperatorToken.IsKind(SyntaxKind.PlusPlusToken))
                {
                    if (PostfixUnaryExpression.Operand is IdentifierNameSyntax IdentifierName)
                    {
                        if (IdentifierName.Identifier.ValueText == localIndex.Name.Text)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}
