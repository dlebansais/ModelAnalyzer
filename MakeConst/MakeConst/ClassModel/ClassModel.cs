namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Z3;
using static System.Net.Mime.MediaTypeNames;

public partial record ClassModel
{
    public required string Name { get; init; }
    public required Dictionary<FieldName, IField> FieldTable { get; init; }
    public required Dictionary<MethodName, IMethod> MethodTable { get; init; }
    public required List<IInvariant> InvariantList { get; init; }
    public required Unsupported Unsupported { get; init; }

    private AutoResetEvent PulseEvent = new(initialState: false);

    public static ClassModel FromClassDeclaration(ClassDeclarationSyntax classDeclaration)
    {
        string Name = classDeclaration.Identifier.ValueText;
        Dictionary<FieldName, IField> FieldTable = new();
        Dictionary<MethodName, IMethod> MethodTable = new();
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

    private static Dictionary<FieldName, IField> ParseFields(ClassDeclarationSyntax classDeclaration, Unsupported unsupported)
    {
        Dictionary<FieldName, IField> FieldTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is FieldDeclarationSyntax FieldDeclaration)
                AddField(FieldDeclaration, FieldTable, unsupported);
    
        return FieldTable;
    }

    private static void AddField(FieldDeclarationSyntax fieldDeclaration, Dictionary<FieldName, IField> fieldTable, Unsupported unsupported)
    {
        VariableDeclarationSyntax Declaration = fieldDeclaration.Declaration;

        if (fieldDeclaration.AttributeLists.Count > 0 ||
            fieldDeclaration.Modifiers.Any(modifier => !modifier.IsKind(SyntaxKind.PrivateKeyword)) ||
            !IsTypeSupported(Declaration.Type, out _))
        {
            Location Location = Declaration.GetLocation();
            UnsupportedField UnsupportedField = new() { FieldName = FieldName.UnsupportedFieldName, Location = Location };
            unsupported.Fields.Add(UnsupportedField);
        }
        else
        {
            foreach (VariableDeclaratorSyntax Variable in Declaration.Variables)
            {
                FieldName FieldName = new(Variable.Identifier.ValueText);
                IField NewField;

                if ((Variable.ArgumentList is not BracketedArgumentListSyntax BracketedArgumentList || BracketedArgumentList.Arguments.Count == 0) &&
                    Variable.Initializer is null)
                {
                    NewField = new Field { FieldName = FieldName };
                }
                else
                {
                    Location Location = Variable.Identifier.GetLocation();
                    UnsupportedField UnsupportedField = new() { FieldName = FieldName, Location = Location };
                    unsupported.Fields.Add(UnsupportedField);

                    NewField = UnsupportedField;
                }

                fieldTable.Add(NewField.FieldName, NewField);
            }
        }
    }

    private static Dictionary<MethodName, IMethod> ParseMethods(ClassDeclarationSyntax classDeclaration, Dictionary<FieldName, IField> fieldTable, Unsupported unsupported)
    {
        Dictionary<MethodName, IMethod> MethodTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
        {
            if (Member.HasLeadingTrivia)
                ReportUnsupportedRequires(unsupported, Member.GetLeadingTrivia());

            if (Member is MethodDeclarationSyntax MethodDeclaration)
                AddMethod(MethodDeclaration, fieldTable, MethodTable, unsupported);
        }

        return MethodTable;
    }

    private static void AddMethod(MethodDeclarationSyntax methodDeclaration, Dictionary<FieldName, IField> fieldTable, Dictionary<MethodName, IMethod> methodTable, Unsupported unsupported)
    {
        MethodName MethodName = new(methodDeclaration.Identifier.ValueText);
        IMethod NewMethod;

        if (IsMethodDeclarationValid(methodDeclaration, out bool HasReturnValue))
        {
            bool IsSupported = true;
            Dictionary<ParameterName, IParameter> ParameterTable = ParseParameters(methodDeclaration, fieldTable, unsupported);
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
            Location Location = methodDeclaration.Identifier.GetLocation();
            UnsupportedMethod UnsupportedMethod = new() { MethodName = MethodName, Location = Location };
            unsupported.Methods.Add(UnsupportedMethod);

            NewMethod = UnsupportedMethod;
        }

        methodTable.Add(NewMethod.MethodName, NewMethod);
    }

    private static bool IsMethodDeclarationValid(MethodDeclarationSyntax methodDeclaration, out bool HasReturnValue)
    {
        HasReturnValue = false;

        if (!IsTypeSupported(methodDeclaration.ReturnType, out bool IsVoidReturn) ||
            methodDeclaration.AttributeLists.Count > 0 ||
            methodDeclaration.Modifiers.Any(modifier => !modifier.IsKind(SyntaxKind.PrivateKeyword) && !modifier.IsKind(SyntaxKind.PublicKeyword) && !modifier.IsKind(SyntaxKind.InternalKeyword)))
            return false;

        HasReturnValue = !IsVoidReturn;
        return true;
    }

    private static Dictionary<ParameterName, IParameter> ParseParameters(MethodDeclarationSyntax methodDeclaration, Dictionary<FieldName, IField> fieldTable, Unsupported unsupported)
    {
        Dictionary<ParameterName, IParameter> ParameterTable = new();

        foreach (ParameterSyntax Parameter in methodDeclaration.ParameterList.Parameters)
        {
            ParameterName ParameterName = new(Parameter.Identifier.ValueText);
            IParameter NewParameter;

            if (IsParameterSupported(Parameter, fieldTable))
            {
                NewParameter = new Parameter() { ParameterName = ParameterName };
            }
            else
            {
                Location Location = Parameter.GetLocation();
                UnsupportedParameter UnsupportedParameter = new UnsupportedParameter() { ParameterName = ParameterName, Location = Location };
                unsupported.Parameters.Add(UnsupportedParameter);

                NewParameter = UnsupportedParameter;
            }

            ParameterTable.Add(ParameterName, NewParameter);
        }

        return ParameterTable;
    }

    private static bool IsParameterSupported(ParameterSyntax parameter, Dictionary<FieldName, IField> fieldTable)
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

    private static List<IRequire> ParseRequires(MethodDeclarationSyntax methodDeclaration, Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported)
    {
        List<IRequire> RequireList;

        if (methodDeclaration.Body is BlockSyntax Block && Block.HasLeadingTrivia)
            RequireList = ParseRequires(Block.GetLeadingTrivia(), fieldTable, parameterTable, unsupported);
        else
            RequireList = new();

        return RequireList;
    }

    private static List<IRequire> ParseRequires(SyntaxTriviaList triviaList, Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported)
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

    private static void ParseRequire(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, List<IRequire> requireList, SyntaxTrivia trivia, string comment, string pattern)
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
            unsupported.Requires.Add(UnsupportedRequire);

            NewRequire = UnsupportedRequire;
        }

        requireList.Add(NewRequire);
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
        unsupported.Requires.Add(UnsupportedRequire);
    }

    private static List<IEnsure> ParseEnsures(MethodDeclarationSyntax methodDeclaration, Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported)
    {
        List<IEnsure> EnsureList = new();

        return EnsureList;
    }

    private static List<IStatement> ParseStatements(MethodDeclarationSyntax methodDeclaration, Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported)
    {
        List<IStatement> StatementList = new();

        if (methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ArrowExpressionClause)
            StatementList = ParseExpressionBody(fieldTable, parameterTable, unsupported, ArrowExpressionClause.Expression);
        else if (methodDeclaration.Body is BlockSyntax Block)
            StatementList = ParseBlock(fieldTable, parameterTable, unsupported, Block, isMainBlock: true);

        return StatementList;
    }

    private static List<IStatement> ParseExpressionBody(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, ExpressionSyntax expressionBody)
    {
        IExpression Expression = ParseExpression(fieldTable, parameterTable, unsupported, expressionBody);
        return new List<IStatement>() { new ReturnStatement { Expression = Expression } };
    }

    private static IExpression ParseExpression(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, ExpressionSyntax expressionNode)
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
            unsupported.Expressions.Add(UnsupportedExpression);

            NewExpression = UnsupportedExpression;
        }

        return NewExpression;
    }

    private static IExpression ParseBinaryExpression(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, BinaryExpressionSyntax expressionNode)
    {
        IExpression? NewExpression = null;
        IExpression Left = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode.Left);
        IExpression Right = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode.Right);

        if (Left is not UnsupportedExpression && Right is not UnsupportedExpression)
        {
            SyntaxKind OperatorKind;

            if (IsBinaryArithmeticOperatorSupported(expressionNode.OperatorToken, out OperatorKind))
                NewExpression = new BinaryArithmeticExpression { Left = Left, OperatorKind = OperatorKind, Right = Right };
            else if (IsComparisonOperatorSupported(expressionNode.OperatorToken, out OperatorKind))
                NewExpression = new ComparisonExpression { Left = Left, OperatorKind = OperatorKind, Right = Right };
            else if (IsBinaryLogicalOperatorSupported(expressionNode.OperatorToken, out OperatorKind))
                NewExpression = new BinaryLogicalExpression { Left = Left, OperatorKind = OperatorKind, Right = Right };
        }

        if (NewExpression is null)
        {
            Location Location = expressionNode.OperatorToken.GetLocation();
            UnsupportedExpression UnsupportedExpression = new() { Location = Location };
            unsupported.Expressions.Add(UnsupportedExpression);

            NewExpression = UnsupportedExpression;
        }

        return NewExpression;
    }

    private static bool IsBinaryArithmeticOperatorSupported(SyntaxToken token, out SyntaxKind operatorKind)
    {
        operatorKind = token.Kind();

        return SupportedArithmeticOperators.ContainsKey(operatorKind);
    }

    private static bool IsComparisonOperatorSupported(SyntaxToken token, out SyntaxKind operatorKind)
    {
        operatorKind = token.Kind();

        return SupportedComparisonOperators.ContainsKey(operatorKind);
    }

    private static bool IsBinaryLogicalOperatorSupported(SyntaxToken token, out SyntaxKind operatorKind)
    {
        operatorKind = token.Kind();

        return SupportedLogicalOperators.ContainsKey(operatorKind);
    }

    private static IExpression ParseParenthesizedExpression(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, ParenthesizedExpressionSyntax expressionNode)
    {
        IExpression NewExpression;
        IExpression Inside = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode.Expression);

        if (Inside is not UnsupportedExpression)
        {
            NewExpression = new ParenthesizedExpression { Inside = Inside };
        }
        else
        {
            Location Location = expressionNode.Expression.GetLocation();
            UnsupportedExpression UnsupportedExpression = new() { Location = Location };
            unsupported.Expressions.Add(UnsupportedExpression);

            NewExpression = UnsupportedExpression;
        }

        return NewExpression;
    }

    private static List<IStatement> ParseStatementOrBlock(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, StatementSyntax node)
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

    private static List<IStatement> ParseBlock(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, BlockSyntax block, bool isMainBlock)
    {
        List<IStatement> StatementList = new();

        foreach (StatementSyntax Item in block.Statements)
        {
            IStatement NewStatement = ParseStatement(fieldTable, parameterTable, unsupported, Item, isMainBlock && Item == block.Statements.Last());
            StatementList.Add(NewStatement);
        }

        return StatementList;
    }

    private static IStatement ParseStatement(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, StatementSyntax node, bool isLastStatement)
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
            unsupported.Statements.Add(UnsupportedStatement);

            NewStatement = UnsupportedStatement;
        }

        return NewStatement;
    }

    private static IStatement ParseAssignmentStatement(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, ExpressionStatementSyntax node)
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
            unsupported.Statements.Add(UnsupportedStatement);

            NewStatement = UnsupportedStatement;
        }

        return NewStatement;
    }

    private static IStatement ParseIfStatement(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, IfStatementSyntax node)
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

    private static IStatement ParseReturnStatement(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, ReturnStatementSyntax node)
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

    private static List<IInvariant> ParseInvariants(ClassDeclarationSyntax classDeclaration, Dictionary<FieldName, IField> fieldTable, Unsupported unsupported)
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

    private static void AddInvariantsInTrivia(List<IInvariant> invariantList, Dictionary<FieldName, IField> fieldTable, Unsupported unsupported, SyntaxTriviaList triviaList)
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

    private static void AddInvariantsInTrivia(List<IInvariant> invariantList, Dictionary<FieldName, IField> fieldTable, Unsupported unsupported, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);
        IInvariant NewInvariant;

        if (TryParseAssertionInTrivia(fieldTable, new Dictionary < ParameterName, IParameter >(), unsupported, Text, out IExpression BooleanExpression))
        {
            NewInvariant = new Invariant { Text = Text, BooleanExpression = BooleanExpression };
        }
        else
        {
            Location Location = GetLocationInComment(trivia, pattern);
            NewInvariant = new UnsupportedInvariant { Text = Text, Location = Location };
        }

        invariantList.Add(NewInvariant);
    }

    private static bool TryParseAssertionInTrivia(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, string text, out IExpression booleanExpression)
    {
        booleanExpression = null!;

        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText($"_ = {text};", Options);
        var Diagnostics = SyntaxTree.GetDiagnostics();
        List<Diagnostic> ErrorList = Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id != "CS1029").ToList();

        return ErrorList.Count == 0 && IsValidInvariantSyntaxTree(fieldTable, parameterTable, unsupported, SyntaxTree, out booleanExpression);
    }

    private static bool IsValidInvariantSyntaxTree(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, SyntaxTree syntaxTree, out IExpression booleanExpression)
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

    private static bool IsValidInvariantExpression(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, Unsupported unsupported, ExpressionSyntax expressionNode, out IExpression booleanExpression)
    {
        booleanExpression = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode);

        return booleanExpression is not UnsupportedExpression;
    }

    private static bool TryFindVariableByName(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, string variableName, out IVariable variable)
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

    private static bool TryFindFieldByName(Dictionary<FieldName, IField> fieldTable, string fieldName, out IField field)
    {
        foreach (KeyValuePair<FieldName, IField> Entry in fieldTable)
            if (Entry.Value is Field ValidField && ValidField.FieldName.Name == fieldName)
            {
                field = ValidField;
                return true;
            }


        field = null!;
        return false;
    }

    private static bool TryFindParameterByName(Dictionary<ParameterName, IParameter> parameterTable, string parameterName, out IParameter parameter)
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

    public override string ToString()
    {
        string Result = @$"{Name}
";

        foreach (KeyValuePair<FieldName, IField> FieldEntry in FieldTable)
            if (FieldEntry.Value is Field Field)
                Result += @$"  int {Field.Name}
";

        foreach (KeyValuePair<MethodName, IMethod> MethodEntry in MethodTable)
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
