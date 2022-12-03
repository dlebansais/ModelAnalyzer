namespace DemoAnalyzer;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Represents the model of a class.
/// </summary>
internal partial record ClassModel : IClassModel
{
    /// <summary>
    /// Gets the class name.
    /// </summary>
    required public string Name { get; init; }

    /// <summary>
    /// Gets the class manager.
    /// </summary>
    required public ClassModelManager Manager { get; init; }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    required public ILogger Logger { get; init; }

    /// <summary>
    /// Gets the field table.
    /// </summary>
    required public FieldTable FieldTable { get; init; }

    /// <summary>
    /// Gets the method table.
    /// </summary>
    required public MethodTable MethodTable { get; init; }

    /// <summary>
    /// Gets the list of invariants.
    /// </summary>
    required public List<IInvariant> InvariantList { get; init; }

    /// <summary>
    /// Gets unsupported class elements.
    /// </summary>
    required public IUnsupported Unsupported { get; init; }

    private AutoResetEvent PulseEvent = new(initialState: false);

    /// <summary>
    /// Creates a class model from a class declaration.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="manager">The class manager.</param>
    /// <param name="logger">The logger.</param>
    public static ClassModel FromClassDeclaration(ClassDeclarationSyntax classDeclaration, ClassModelManager manager, ILogger logger)
    {
        string Name = classDeclaration.Identifier.ValueText;
        FieldTable FieldTable = new();
        MethodTable MethodTable = new();
        List<IInvariant> InvariantList = new();
        Unsupported Unsupported = new();

        if (Name == string.Empty || !IsClassDeclarationSupported(classDeclaration))
            Unsupported.InvalidDeclaration = true;
        else
        {
            Unsupported.HasUnsupporteMember = CheckUnsupportedMembers(classDeclaration);
            FieldTable = ParseFields(classDeclaration, Unsupported);
            MethodTable = ParseMethods(classDeclaration, FieldTable, Unsupported);
            InvariantList = ParseInvariants(classDeclaration, FieldTable, Unsupported);
        }

        return new ClassModel
        {
            Name = Name,
            Manager = manager,
            Logger = logger,
            FieldTable = FieldTable,
            MethodTable = MethodTable,
            InvariantList = InvariantList,
            Unsupported = Unsupported,
        };
    }

    private static bool IsClassDeclarationSupported(ClassDeclarationSyntax classDeclaration)
    {
        if (classDeclaration.AttributeLists.Count > 0)
            return false;

        foreach (SyntaxToken Modifier in classDeclaration.Modifiers)
            if (!Modifier.IsKind(SyntaxKind.PrivateKeyword) && !Modifier.IsKind(SyntaxKind.PublicKeyword) && !Modifier.IsKind(SyntaxKind.InternalKeyword) && !Modifier.IsKind(SyntaxKind.PartialKeyword))
                return false;

        if (classDeclaration.BaseList is BaseListSyntax BaseList && BaseList.Types.Count > 0)
            return false;

        if (classDeclaration.TypeParameterList is TypeParameterListSyntax TypeParameterList && TypeParameterList.Parameters.Count > 0)
            return false;

        if (classDeclaration.ConstraintClauses.Count > 0)
            return false;

        return true;
    }

    private static bool CheckUnsupportedMembers(ClassDeclarationSyntax classDeclaration)
    {
        bool HasUnsupportedNode = false;

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is not FieldDeclarationSyntax && Member is not MethodDeclarationSyntax)
                HasUnsupportedNode = true;

        return HasUnsupportedNode;
    }

    private static FieldTable ParseFields(ClassDeclarationSyntax classDeclaration, Unsupported unsupported)
    {
        FieldTable FieldTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is FieldDeclarationSyntax FieldDeclaration)
                AddField(FieldDeclaration, FieldTable, unsupported);

        return FieldTable;
    }

    private static void AddField(FieldDeclarationSyntax fieldDeclaration, FieldTable fieldTable, Unsupported unsupported)
    {
        VariableDeclarationSyntax Declaration = fieldDeclaration.Declaration;

        if (fieldDeclaration.AttributeLists.Count > 0 ||
            fieldDeclaration.Modifiers.Any(modifier => !modifier.IsKind(SyntaxKind.PrivateKeyword)) ||
            !IsTypeSupported(Declaration.Type, out _))
        {
            Location Location = Declaration.GetLocation();
            UnsupportedField UnsupportedField = new() { Location = Location };
            unsupported.AddUnsupportedField(UnsupportedField);
        }
        else
        {
            foreach (VariableDeclaratorSyntax Variable in Declaration.Variables)
                AddField(Variable, fieldTable, unsupported);
        }
    }

    private static void AddField(VariableDeclaratorSyntax variable, FieldTable fieldTable, Unsupported unsupported)
    {
        FieldName FieldName = new(variable.Identifier.ValueText);

        if (!fieldTable.ContainsField(FieldName))
        {
            IField NewField;

            if ((variable.ArgumentList is not BracketedArgumentListSyntax BracketedArgumentList || BracketedArgumentList.Arguments.Count == 0) &&
                variable.Initializer is null)
            {
                NewField = new Field { FieldName = FieldName };
            }
            else
            {
                Location Location = variable.Identifier.GetLocation();
                UnsupportedField UnsupportedField = new() { Location = Location };
                unsupported.AddUnsupportedField(UnsupportedField);

                NewField = UnsupportedField;
            }

            fieldTable.AddField(FieldName, NewField);
        }
    }

    private static MethodTable ParseMethods(ClassDeclarationSyntax classDeclaration, FieldTable fieldTable, Unsupported unsupported)
    {
        MethodTable MethodTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
        {
            SyntaxTriviaList TriviaList;

            if (TryFindLeadingTrivia(Member, out TriviaList))
                ReportUnsupportedRequires(unsupported, TriviaList);

            if (Member is MethodDeclarationSyntax MethodDeclaration)
                AddMethod(MethodDeclaration, fieldTable, MethodTable, unsupported);
            else if (TryFindTrailingTrivia(Member, out TriviaList))
                ReportUnsupportedEnsures(unsupported, TriviaList);
        }

        return MethodTable;
    }

    private static void AddMethod(MethodDeclarationSyntax methodDeclaration, FieldTable fieldTable, MethodTable methodTable, Unsupported unsupported)
    {
        string Name = methodDeclaration.Identifier.ValueText;
        MethodName MethodName = new(Name);

        if (!methodTable.ContainsMethod(MethodName))
        {
            IMethod NewMethod;

            if (IsMethodDeclarationValid(methodDeclaration, out bool HasReturnValue))
            {
                bool IsSupported = true;
                ParameterTable ParameterTable = ParseParameters(methodDeclaration, fieldTable, unsupported);
                List<IRequire> RequireList = ParseRequires(methodDeclaration, fieldTable, ParameterTable, unsupported);
                List<IStatement> StatementList = ParseStatements(methodDeclaration, fieldTable, ParameterTable, unsupported);
                List<IEnsure> EnsureList = ParseEnsures(methodDeclaration, fieldTable, ParameterTable, unsupported);

                NewMethod = new Method
                {
                    MethodName = MethodName,
                    IsSupported = IsSupported,
                    HasReturnValue = HasReturnValue,
                    ParameterTable = ParameterTable,
                    RequireList = RequireList,
                    StatementList = StatementList,
                    EnsureList = EnsureList,
                };
            }
            else
            {
                if (TryFindTrailingTrivia(methodDeclaration, out SyntaxTriviaList TriviaList))
                    ReportUnsupportedEnsures(unsupported, TriviaList);

                Location Location = methodDeclaration.Identifier.GetLocation();
                UnsupportedMethod UnsupportedMethod = new() { MethodName = MethodName, Location = Location };
                unsupported.AddUnsupportedMethod(UnsupportedMethod);

                NewMethod = UnsupportedMethod;
            }

            methodTable.AddMethod(NewMethod.MethodName, NewMethod);
        }
    }

    private static bool IsMethodDeclarationValid(MethodDeclarationSyntax methodDeclaration, out bool hasReturnValue)
    {
        hasReturnValue = false;

        if (!IsTypeSupported(methodDeclaration.ReturnType, out bool IsVoidReturn) ||
            methodDeclaration.AttributeLists.Count > 0 ||
            methodDeclaration.Modifiers.Any(modifier => !modifier.IsKind(SyntaxKind.PrivateKeyword) && !modifier.IsKind(SyntaxKind.PublicKeyword) && !modifier.IsKind(SyntaxKind.InternalKeyword)))
            return false;

        hasReturnValue = !IsVoidReturn;
        return true;
    }

    private static ParameterTable ParseParameters(MethodDeclarationSyntax methodDeclaration, FieldTable fieldTable, Unsupported unsupported)
    {
        ParameterTable ParameterTable = new();

        foreach (ParameterSyntax Parameter in methodDeclaration.ParameterList.Parameters)
        {
            ParameterName ParameterName = new(Parameter.Identifier.ValueText);

            if (!ParameterTable.ContainsParameter(ParameterName))
            {
                IParameter NewParameter;

                if (IsParameterSupported(Parameter, fieldTable))
                {
                    NewParameter = new Parameter() { ParameterName = ParameterName };
                }
                else
                {
                    Location Location = Parameter.GetLocation();
                    UnsupportedParameter UnsupportedParameter = new UnsupportedParameter() { ParameterName = ParameterName, Location = Location };
                    unsupported.AddUnsupportedParameter(UnsupportedParameter);

                    NewParameter = UnsupportedParameter;
                }

                ParameterTable.AddParameter(ParameterName, NewParameter);
            }
        }

        return ParameterTable;
    }

    private static bool IsParameterSupported(ParameterSyntax parameter, FieldTable fieldTable)
    {
        if (parameter.AttributeLists.Count > 0)
            return false;

        if (parameter.Modifiers.Count > 0)
            return false;

        if (!IsTypeSupported(parameter.Type, out _))
            return false;

        if (TryFindFieldByName(fieldTable, parameter.Identifier.ValueText, out _))
            return false;

        return true;
    }

    private static List<IRequire> ParseRequires(MethodDeclarationSyntax methodDeclaration, FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported)
    {
        List<IRequire> RequireList;

        if (methodDeclaration.Body is BlockSyntax Block && Block.HasLeadingTrivia)
            RequireList = ParseRequires(Block.GetLeadingTrivia(), fieldTable, parameterTable, unsupported);
        else
            RequireList = new();

        return RequireList;
    }

    private static List<IRequire> ParseRequires(SyntaxTriviaList triviaList, FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported)
    {
        List<IRequire> RequireList = new();

        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Require}";

                if (Comment.StartsWith(Pattern))
                    ParseRequire(fieldTable, parameterTable, unsupported, RequireList, Trivia, Comment, Pattern);
            }

        return RequireList;
    }

    private static void ParseRequire(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, List<IRequire> requireList, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);
        IRequire NewRequire;

        if (TryParseAssertionInTrivia(fieldTable, parameterTable, unsupported, Text, out IExpression BooleanExpression))
        {
            NewRequire = new Require { Text = Text, BooleanExpression = BooleanExpression };
        }
        else
        {
            Location Location = GetLocationInComment(trivia, pattern);
            UnsupportedRequire UnsupportedRequire = new UnsupportedRequire { Text = Text, Location = Location };
            unsupported.AddUnsupportedRequire(UnsupportedRequire);

            NewRequire = UnsupportedRequire;
        }

        requireList.Add(NewRequire);
    }

    private static bool TryFindLeadingTrivia(MemberDeclarationSyntax member, out SyntaxTriviaList triviaList)
    {
        if (member.HasLeadingTrivia)
        {
            triviaList = member.GetLeadingTrivia();
            return true;
        }

        triviaList = default;
        return false;
    }

    private static bool TryFindTrailingTrivia(MemberDeclarationSyntax member, out SyntaxTriviaList triviaList)
    {
        SyntaxToken LastToken = member.GetLastToken();
        SyntaxToken NextToken = LastToken.GetNextToken();

        if (NextToken.HasLeadingTrivia)
        {
            triviaList = NextToken.LeadingTrivia;
            return true;
        }

        triviaList = default;
        return false;
    }

    private static void ReportUnsupportedRequires(Unsupported unsupported, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Require}";

                if (Comment.StartsWith(Pattern))
                    ReportUnsupportedRequire(unsupported, Trivia, Comment, Pattern);
            }
    }

    private static void ReportUnsupportedRequire(Unsupported unsupported, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);
        Location Location = GetLocationInComment(trivia, pattern);
        UnsupportedRequire UnsupportedRequire = new() { Text = Text, Location = Location };
        unsupported.AddUnsupportedRequire(UnsupportedRequire);
    }

    private static List<IEnsure> ParseEnsures(MethodDeclarationSyntax methodDeclaration, FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported)
    {
        List<IEnsure> EnsureList;

        SyntaxToken LastToken = methodDeclaration.GetLastToken();
        SyntaxToken NextToken = LastToken.GetNextToken();

        if (NextToken.HasLeadingTrivia)
            EnsureList = ParseEnsures(NextToken.LeadingTrivia, fieldTable, parameterTable, unsupported);
        else
            EnsureList = new();

        return EnsureList;
    }

    private static List<IEnsure> ParseEnsures(SyntaxTriviaList triviaList, FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported)
    {
        List<IEnsure> EnsureList = new();

        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Ensure}";

                if (Comment.StartsWith(Pattern))
                    ParseEnsure(fieldTable, parameterTable, unsupported, EnsureList, Trivia, Comment, Pattern);
            }

        return EnsureList;
    }

    private static void ParseEnsure(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, List<IEnsure> ensureList, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);
        IEnsure NewEnsure;

        if (TryParseAssertionInTrivia(fieldTable, parameterTable, unsupported, Text, out IExpression BooleanExpression))
        {
            NewEnsure = new Ensure { Text = Text, BooleanExpression = BooleanExpression };
        }
        else
        {
            Location Location = GetLocationInComment(trivia, pattern);
            UnsupportedEnsure UnsupportedEnsure = new UnsupportedEnsure { Text = Text, Location = Location };
            unsupported.AddUnsupportedEnsure(UnsupportedEnsure);

            NewEnsure = UnsupportedEnsure;
        }

        ensureList.Add(NewEnsure);
    }

    private static void ReportUnsupportedEnsures(Unsupported unsupported, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Ensure}";

                if (Comment.StartsWith(Pattern))
                    ReportUnsupportedEnsure(unsupported, Trivia, Comment, Pattern);
            }
    }

    private static void ReportUnsupportedEnsure(Unsupported unsupported, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);
        Location Location = GetLocationInComment(trivia, pattern);
        UnsupportedEnsure UnsupportedEnsure = new() { Text = Text, Location = Location };
        unsupported.AddUnsupportedEnsure(UnsupportedEnsure);
    }

    private static List<IStatement> ParseStatements(MethodDeclarationSyntax methodDeclaration, FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported)
    {
        List<IStatement> StatementList = new();

        if (methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ArrowExpressionClause)
            StatementList = ParseExpressionBody(fieldTable, parameterTable, unsupported, ArrowExpressionClause.Expression);
        else if (methodDeclaration.Body is BlockSyntax Block)
            StatementList = ParseBlock(fieldTable, parameterTable, unsupported, Block, isMainBlock: true);

        return StatementList;
    }

    private static List<IStatement> ParseExpressionBody(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionSyntax expressionBody)
    {
        IExpression Expression = ParseExpression(fieldTable, parameterTable, unsupported, expressionBody);
        return new List<IStatement>() { new ReturnStatement { Expression = Expression } };
    }

    private static IExpression ParseExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionSyntax expressionNode)
    {
        IExpression NewExpression;

        if (expressionNode is BinaryExpressionSyntax BinaryExpression)
            NewExpression = ParseBinaryExpression(fieldTable, parameterTable, unsupported, BinaryExpression);
        else if (expressionNode is IdentifierNameSyntax IdentifierName && TryFindVariableByName(fieldTable, parameterTable, IdentifierName.Identifier.ValueText, out IVariable Variable))
            NewExpression = new VariableValueExpression { Variable = Variable };
        else if (expressionNode is LiteralExpressionSyntax LiteralExpression && int.TryParse(LiteralExpression.Token.ValueText, out int Value))
            NewExpression = new LiteralValueExpression { Value = Value };
        else if (expressionNode is ParenthesizedExpressionSyntax ParenthesizedExpression)
            NewExpression = ParseParenthesizedExpression(fieldTable, parameterTable, unsupported, ParenthesizedExpression);
        else
        {
            Location Location = expressionNode.GetLocation();
            UnsupportedExpression UnsupportedExpression = new() { Location = Location };
            unsupported.AddUnsupportedExpression(UnsupportedExpression);

            NewExpression = UnsupportedExpression;
        }

        return NewExpression;
    }

    private static IExpression ParseBinaryExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, BinaryExpressionSyntax expressionNode)
    {
        IExpression? NewExpression = null;
        IExpression LeftExpression = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode.Left);
        IExpression RightExpression = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode.Right);

        if (LeftExpression is Expression Left && RightExpression is Expression Right)
        {
            if (IsBinaryArithmeticOperatorSupported(expressionNode.OperatorToken, out ArithmeticOperator ArithmeticOperator))
                NewExpression = new BinaryArithmeticExpression { Left = Left, Operator = ArithmeticOperator, Right = Right };
            else if (IsComparisonOperatorSupported(expressionNode.OperatorToken, out ComparisonOperator ComparisonOperator))
                NewExpression = new ComparisonExpression { Left = Left, Operator = ComparisonOperator, Right = Right };
            else if (IsBinaryLogicalOperatorSupported(expressionNode.OperatorToken, out LogicalOperator LogicalOperator))
                NewExpression = new BinaryLogicalExpression { Left = Left, Operator = LogicalOperator, Right = Right };
        }

        if (NewExpression is null)
        {
            Location Location = expressionNode.OperatorToken.GetLocation();
            UnsupportedExpression UnsupportedExpression = new() { Location = Location };
            unsupported.AddUnsupportedExpression(UnsupportedExpression);

            NewExpression = UnsupportedExpression;
        }

        return NewExpression;
    }

    private static bool IsBinaryArithmeticOperatorSupported(SyntaxToken token, out ArithmeticOperator arithmeticOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (SupportedOperators.Arithmetic.ContainsKey(OperatorKind))
        {
            arithmeticOperator = SupportedOperators.Arithmetic[OperatorKind];
            return true;
        }

        arithmeticOperator = null!;
        return false;
    }

    private static bool IsComparisonOperatorSupported(SyntaxToken token, out ComparisonOperator comparisonOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (SupportedOperators.Comparison.ContainsKey(OperatorKind))
        {
            comparisonOperator = SupportedOperators.Comparison[OperatorKind];
            return true;
        }

        comparisonOperator = null!;
        return false;
    }

    private static bool IsBinaryLogicalOperatorSupported(SyntaxToken token, out LogicalOperator logicalOperator)
    {
        SyntaxKind OperatorKind = token.Kind();

        if (SupportedOperators.Logical.ContainsKey(OperatorKind))
        {
            logicalOperator = SupportedOperators.Logical[OperatorKind];
            return true;
        }

        logicalOperator = null!;
        return false;
    }

    private static IExpression ParseParenthesizedExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ParenthesizedExpressionSyntax expressionNode)
    {
        IExpression NewExpression;
        IExpression InsideExpression = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode.Expression);

        if (InsideExpression is Expression Inside)
        {
            NewExpression = new ParenthesizedExpression { Inside = Inside };
        }
        else
        {
            Location Location = expressionNode.Expression.GetLocation();
            UnsupportedExpression UnsupportedExpression = new() { Location = Location };
            unsupported.AddUnsupportedExpression(UnsupportedExpression);

            NewExpression = UnsupportedExpression;
        }

        return NewExpression;
    }

    private static List<IStatement> ParseStatementOrBlock(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, StatementSyntax node)
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

    private static List<IStatement> ParseBlock(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, BlockSyntax block, bool isMainBlock)
    {
        List<IStatement> StatementList = new();

        foreach (StatementSyntax Item in block.Statements)
        {
            IStatement NewStatement = ParseStatement(fieldTable, parameterTable, unsupported, Item, isMainBlock && Item == block.Statements.Last());
            StatementList.Add(NewStatement);
        }

        return StatementList;
    }

    private static IStatement ParseStatement(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, StatementSyntax node, bool isLastStatement)
    {
        IStatement? NewStatement = null;

        if (node.AttributeLists.Count == 0)
        {
            if (node is ExpressionStatementSyntax ExpressionStatement)
                NewStatement = ParseAssignmentStatement(fieldTable, parameterTable, unsupported, ExpressionStatement);
            else if (node is IfStatementSyntax IfStatement)
                NewStatement = ParseIfStatement(fieldTable, parameterTable, unsupported, IfStatement);
            else if (node is ReturnStatementSyntax ReturnStatement && isLastStatement)
                NewStatement = ParseReturnStatement(fieldTable, parameterTable, unsupported, ReturnStatement);
        }

        if (NewStatement is null)
        {
            Location Location = node.GetLocation();
            UnsupportedStatement UnsupportedStatement = new() { Location = Location };
            unsupported.AddUnsupportedStatement(UnsupportedStatement);

            NewStatement = UnsupportedStatement;
        }

        return NewStatement;
    }

    private static IStatement ParseAssignmentStatement(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionStatementSyntax node)
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
            Location Location = node.GetLocation();
            UnsupportedStatement UnsupportedStatement = new() { Location = Location };
            unsupported.AddUnsupportedStatement(UnsupportedStatement);

            NewStatement = UnsupportedStatement;
        }

        return NewStatement;
    }

    private static IStatement ParseIfStatement(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, IfStatementSyntax node)
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

    private static IStatement ParseReturnStatement(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ReturnStatementSyntax node)
    {
        IExpression? ReturnExpression;

        if (node.Expression is not null)
            ReturnExpression = ParseExpression(fieldTable, parameterTable, unsupported, node.Expression);
        else
            ReturnExpression = null;

        return new ReturnStatement { Expression = ReturnExpression };
    }

    private static bool IsTypeSupported(TypeSyntax? type, out bool isVoid)
    {
        isVoid = false;

        if (type is not PredefinedTypeSyntax PredefinedType)
            return false;

        SyntaxKind TypeKind = PredefinedType.Keyword.Kind();
        if (TypeKind != SyntaxKind.IntKeyword && TypeKind != SyntaxKind.VoidKeyword)
            return false;

        isVoid = TypeKind == SyntaxKind.VoidKeyword;
        return true;
    }

    private static List<IInvariant> ParseInvariants(ClassDeclarationSyntax classDeclaration, FieldTable fieldTable, Unsupported unsupported)
    {
        List<IInvariant> InvariantList = new();

        SyntaxToken LastToken = classDeclaration.GetLastToken();
        var Location = LastToken.GetLocation();

        if (LastToken.HasLeadingTrivia)
            ReportUnsupportedRequires(unsupported, LastToken.LeadingTrivia);

        if (LastToken.HasTrailingTrivia)
        {
            SyntaxTriviaList TrailingTrivia = LastToken.TrailingTrivia;
            AddInvariantsInTrivia(InvariantList, fieldTable, unsupported, TrailingTrivia);
            Location = TrailingTrivia.Last().GetLocation();
        }

        var NextToken = classDeclaration.SyntaxTree.GetRoot().FindToken(Location.SourceSpan.End);

        if (NextToken.HasLeadingTrivia)
            AddInvariantsInTrivia(InvariantList, fieldTable, unsupported, NextToken.LeadingTrivia);

        return InvariantList;
    }

    private static void AddInvariantsInTrivia(List<IInvariant> invariantList, FieldTable fieldTable, Unsupported unsupported, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Invariant}";

                if (Comment.StartsWith(Pattern))
                    AddInvariantsInTrivia(invariantList, fieldTable, unsupported, Trivia, Comment, Pattern);
            }
    }

    private static void AddInvariantsInTrivia(List<IInvariant> invariantList, FieldTable fieldTable, Unsupported unsupported, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);
        IInvariant NewInvariant;

        if (TryParseAssertionInTrivia(fieldTable, new ParameterTable(), unsupported, Text, out IExpression BooleanExpression))
        {
            NewInvariant = new Invariant { Text = Text, BooleanExpression = BooleanExpression };
        }
        else
        {
            Location Location = GetLocationInComment(trivia, pattern);
            UnsupportedInvariant UnsupportedInvariant = new UnsupportedInvariant { Text = Text, Location = Location };
            unsupported.AddUnsupportedInvariant(UnsupportedInvariant);

            NewInvariant = UnsupportedInvariant;
        }

        invariantList.Add(NewInvariant);
    }

    private static bool TryParseAssertionInTrivia(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, string text, out IExpression booleanExpression)
    {
        booleanExpression = null!;

        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText($"_ = {text};", Options);
        var Diagnostics = SyntaxTree.GetDiagnostics();
        List<Diagnostic> ErrorList = Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id != "CS1029").ToList();

        return ErrorList.Count == 0 && IsValidInvariantSyntaxTree(fieldTable, parameterTable, unsupported, SyntaxTree, out booleanExpression);
    }

    private static bool IsValidInvariantSyntaxTree(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, SyntaxTree syntaxTree, out IExpression booleanExpression)
    {
        booleanExpression = null!;

        CompilationUnitSyntax Root = syntaxTree.GetCompilationUnitRoot();
        if (Root.AttributeLists.Count > 0 || Root.Usings.Count > 0 || Root.Members.Count != 1)
            return false;

        if (Root.Members[0] is not GlobalStatementSyntax GlobalStatement ||
            GlobalStatement.Statement is not ExpressionStatementSyntax ExpressionStatement ||
            ExpressionStatement.Expression is not AssignmentExpressionSyntax AssignmentExpression)
            return false;

        ExpressionSyntax Expression = AssignmentExpression.Right;
        return IsValidInvariantExpression(fieldTable, parameterTable, unsupported, Expression, out booleanExpression);
    }

    private static bool IsValidInvariantExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionSyntax expressionNode, out IExpression booleanExpression)
    {
        booleanExpression = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode);

        return booleanExpression is not UnsupportedExpression;
    }

    private static bool TryFindVariableByName(FieldTable fieldTable, ParameterTable parameterTable, string variableName, out IVariable variable)
    {
        if (TryFindFieldByName(fieldTable, variableName, out IField Field))
        {
            variable = Field;
            return true;
        }

        if (TryFindParameterByName(parameterTable, variableName, out IParameter Parameter))
        {
            variable = Parameter;
            return true;
        }

        variable = null!;
        return false;
    }

    private static bool TryFindFieldByName(FieldTable fieldTable, string fieldName, out IField field)
    {
        foreach (KeyValuePair<IFieldName, IField> Entry in fieldTable)
            if (Entry.Value is Field ValidField && ValidField.FieldName.Name == fieldName)
            {
                field = ValidField;
                return true;
            }

        field = null!;
        return false;
    }

    private static bool TryFindParameterByName(ParameterTable parameterTable, string parameterName, out IParameter parameter)
    {
        foreach (KeyValuePair<ParameterName, IParameter> Entry in parameterTable)
            if (Entry.Value is Parameter ValidParameter && ValidParameter.ParameterName.Name == parameterName)
            {
                parameter = ValidParameter;
                return true;
            }

        parameter = null!;
        return false;
    }

    private static Location GetLocationInComment(SyntaxTrivia trivia, string pattern)
    {
        Location FullLocation = trivia.GetLocation();
        TextSpan FullSpan = FullLocation.SourceSpan;
        TextSpan InvariantSpan = new TextSpan(FullSpan.Start + pattern.Length, FullSpan.Length - pattern.Length);
        Location Location = Location.Create(FullLocation.SourceTree!, InvariantSpan);

        return Location;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string Result = @$"{Name}
";

        foreach (KeyValuePair<IFieldName, IField> FieldEntry in FieldTable)
            if (FieldEntry.Value is Field Field)
                Result += @$"  int {Field.Name}
";

        foreach (KeyValuePair<IMethodName, IMethod> MethodEntry in MethodTable)
            if (MethodEntry.Value is Method Method)
            {
                string Parameters = string.Empty;

                foreach (KeyValuePair<ParameterName, IParameter> ParameterEntry in Method.ParameterTable)
                {
                    if (Parameters.Length > 0)
                        Parameters += ", ";

                    Parameters += ParameterEntry.Key.Name;
                }

                string ReturnString = Method.HasReturnValue ? "int" : "void";
                Result += @$"  {ReturnString} {Method.MethodName.Name}({Parameters})
";

                foreach (Invariant Invariant in InvariantList)
                {
                    Result += @$"  * {Invariant.BooleanExpression}
";
                }
            }

        return Result;
    }
}
