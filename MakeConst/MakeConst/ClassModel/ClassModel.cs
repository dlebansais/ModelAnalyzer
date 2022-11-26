namespace DemoAnalyzer;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

public partial record ClassModel
{
    public required string Name { get; init; }
    public required bool IsSupported { get; init; }
    public required Dictionary<FieldName, IField> FieldTable { get; init; }
    public required Dictionary<MethodName, IMethod> MethodTable { get; init; }
    public required List<IInvariant> InvariantList { get; init; }

    public static bool IsClassIgnoredForModeling(ClassDeclarationSyntax classDeclaration)
    {
        SyntaxToken firstToken = classDeclaration.GetFirstToken();
        SyntaxTriviaList leadingTrivia = firstToken.LeadingTrivia;

        foreach (SyntaxTrivia Trivia in leadingTrivia)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                if (Comment.StartsWith($"// {Modeling.None}"))
                    return true;
            }

        return false;
    }

    public static ClassModel FromClassDeclaration(ClassDeclarationSyntax classDeclaration)
    {
        string Name = classDeclaration.Identifier.ValueText;
        bool IsSupported = false;
        Dictionary<FieldName, IField> FieldTable = new();
        Dictionary<MethodName, IMethod> MethodTable = new();
        List<IInvariant> InvariantList = new();

        if (Name != string.Empty && IsClassDeclarationSupported(classDeclaration))
        {
            IsSupported = true;
            CheckUnsupportedMembers(classDeclaration, ref IsSupported);
            FieldTable = ParseFields(classDeclaration, ref IsSupported);
            MethodTable = ParseMethods(classDeclaration, FieldTable, ref IsSupported);
            InvariantList = ParseInvariants(classDeclaration, FieldTable);
        }

        return new ClassModel
        {
            Name = Name,
            IsSupported = IsSupported,
            FieldTable = FieldTable,
            MethodTable = MethodTable,
            InvariantList = InvariantList,
        };
    }

    private static bool IsClassDeclarationSupported(ClassDeclarationSyntax classDeclaration)
    {
        if (classDeclaration.AttributeLists.Count > 0)
        {
            Logger.Log("Too many attributes");
            return false;
        }

        foreach (SyntaxToken Modifier in classDeclaration.Modifiers)
            if (!Modifier.IsKind(SyntaxKind.PrivateKeyword) && !Modifier.IsKind(SyntaxKind.PublicKeyword) && !Modifier.IsKind(SyntaxKind.InternalKeyword) && !Modifier.IsKind(SyntaxKind.PartialKeyword))
            {
                Logger.Log("Unsupported modifier");
                return false;
            }

        if (classDeclaration.BaseList is BaseListSyntax BaseList && BaseList.Types.Count > 0)
        {
            Logger.Log("Unsupported base");
            return false;
        }

        if (classDeclaration.TypeParameterList is TypeParameterListSyntax TypeParameterList && TypeParameterList.Parameters.Count > 0)
        {
            Logger.Log("Unsupported type parameters");
            return false;
        }

        if (classDeclaration.ConstraintClauses.Count > 0)
        {
            Logger.Log("Unsupported constraints");
            return false;
        }

        return true;
    }

    private static void CheckUnsupportedMembers(ClassDeclarationSyntax classDeclaration, ref bool isSupported)
    {
        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is not FieldDeclarationSyntax && Member is not MethodDeclarationSyntax)
            {
                Logger.Log($"{Member.GetType()} not supported");
                isSupported = false;
                break;
            }
    }

    private static Dictionary<FieldName, IField> ParseFields(ClassDeclarationSyntax classDeclaration, ref bool isSupported)
    {
        Dictionary<FieldName, IField> FieldTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is FieldDeclarationSyntax FieldDeclaration)
                AddField(FieldDeclaration, FieldTable, ref isSupported);
    
        return FieldTable;
    }

    private static void AddField(FieldDeclarationSyntax fieldDeclaration, Dictionary<FieldName, IField> fieldTable, ref bool isSupported)
    {
        VariableDeclarationSyntax Declaration = fieldDeclaration.Declaration;

        bool IsFieldSupported = (fieldDeclaration.AttributeLists.Count == 0 &&
                                 fieldDeclaration.Modifiers.All(modifier => modifier.IsKind(SyntaxKind.PrivateKeyword)) &&
                                 IsTypeSupported(Declaration.Type, out _));

        foreach (VariableDeclaratorSyntax Variable in Declaration.Variables)
        {
            FieldName FieldName = new(Variable.Identifier.ValueText);
            IField NewField;

            if (IsFieldSupported &&
                (Variable.ArgumentList is not BracketedArgumentListSyntax BracketedArgumentList || BracketedArgumentList.Arguments.Count == 0) &&
                Variable.Initializer is null)
            {
                NewField = new Field { FieldName = FieldName };
            }
            else
            {
                Logger.Log($"Bad field: {FieldName.Name}");

                Location Location = Variable.Identifier.GetLocation();
                NewField = new UnsupportedField { FieldName = FieldName, Location = Location };
                isSupported = false;
            }

            fieldTable.Add(NewField.FieldName, NewField);
        }
    }

    private static Dictionary<MethodName, IMethod> ParseMethods(ClassDeclarationSyntax classDeclaration, Dictionary<FieldName, IField> fieldTable, ref bool isSupported)
    {
        Dictionary<MethodName, IMethod> MethodTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is MethodDeclarationSyntax MethodDeclaration)
                AddMethod(MethodDeclaration, fieldTable, MethodTable, ref isSupported);

        return MethodTable;
    }

    private static void AddMethod(MethodDeclarationSyntax methodDeclaration, Dictionary<FieldName, IField> fieldTable, Dictionary<MethodName, IMethod> methodTable, ref bool isSupported)
    {
        MethodName MethodName = new(methodDeclaration.Identifier.ValueText);
        IMethod NewMethod;

        if (IsMethodSupported(methodDeclaration, fieldTable, out bool HasReturnValue, out Dictionary<ParameterName, IParameter> ParameterTable, out List<IStatement> StatementList))
        {
            NewMethod = new Method { MethodName = MethodName, HasReturnValue = HasReturnValue, ParameterTable = ParameterTable, StatementList = StatementList };
        }
        else
        {
            Logger.Log($"Bad method: {MethodName.Name}");

            Location Location = methodDeclaration.Identifier.GetLocation();
            NewMethod = new UnsupportedMethod { MethodName = MethodName, Location = Location };
            isSupported = false;
        }

        methodTable.Add(NewMethod.MethodName, NewMethod);
    }

    private static bool IsMethodSupported(MethodDeclarationSyntax methodDeclaration, Dictionary<FieldName, IField> fieldTable, out bool HasReturnValue, out Dictionary<ParameterName, IParameter> parameterTable, out List<IStatement> statementList)
    {
        HasReturnValue = false;
        parameterTable = null!;
        statementList = null!;

        if (!IsTypeSupported(methodDeclaration.ReturnType, out bool IsVoidReturn) ||
            methodDeclaration.AttributeLists.Count > 0 ||
            methodDeclaration.Modifiers.Any(modifier => !modifier.IsKind(SyntaxKind.PrivateKeyword) && !modifier.IsKind(SyntaxKind.PublicKeyword) && !modifier.IsKind(SyntaxKind.InternalKeyword)))
            return false;

        if (!AreAllMethodParametersSupported(methodDeclaration, out parameterTable))
            return false;

        HasReturnValue = !IsVoidReturn;

        if (HasReturnValue && methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ArrowExpressionClause)
        {
            if (!IsExpressionSupported(fieldTable, parameterTable, ArrowExpressionClause.Expression, out IExpression Expression))
                return false;

            ReturnStatement Statement = new() { Expression = Expression };
            statementList = new List<IStatement>() { Statement };
        }
        else if (methodDeclaration.Body is BlockSyntax Block)
        {
            if (!IsBlockSupported(fieldTable, parameterTable, Block, out statementList))
                return false;
        }

        return true;
    }

    private static bool AreAllMethodParametersSupported(MethodDeclarationSyntax method, out Dictionary<ParameterName, IParameter> parameterTable)
    {
        parameterTable = new Dictionary<ParameterName, IParameter>();

        foreach (ParameterSyntax Parameter in method.ParameterList.Parameters)
            if (!IsParameterSupported(Parameter))
                return false;
            else
            {
                ParameterName ParameterName = new(Parameter.Identifier.ValueText);
                parameterTable.Add(ParameterName, new Parameter() { ParameterName = ParameterName });
            }

        return true;
    }

    private static bool IsParameterSupported(ParameterSyntax parameter)
    {
        if (parameter.AttributeLists.Count > 0)
            return false;

        if (parameter.Modifiers.Count > 0)
            return false;

        if (!IsTypeSupported(parameter.Type, out _))
            return false;

        return true;
    }

    private static bool IsExpressionSupported(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, ExpressionSyntax expressionNode, out IExpression expression)
    {
        if (expressionNode is BinaryExpressionSyntax BinaryExpression)
            return IsBinaryExpressionSupported(fieldTable, parameterTable, BinaryExpression, out expression);
        else if (expressionNode is IdentifierNameSyntax IdentifierName && TryFindVariableByName(fieldTable, parameterTable, IdentifierName.Identifier.ValueText, out IVariable Variable))
        {
            expression = new VariableValueExpression { Variable = Variable };
            return true;
        }
        else if (expressionNode is LiteralExpressionSyntax LiteralExpression && int.TryParse(LiteralExpression.Token.ValueText, out int Value))
        {
            expression = new LiteralValueExpression { Value = Value };
            return true;
        }

        expression = null!;
        return false;
    }

    private static bool IsAssignmentExpressionSupported(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, AssignmentExpressionSyntax expressionNode, out IField destination, out IExpression expression)
    {
        if (expressionNode.Left is IdentifierNameSyntax IdentifierName &&
            TryFindFieldByName(fieldTable, IdentifierName.Identifier.ValueText, out destination) &&
            IsExpressionSupported(fieldTable, parameterTable, expressionNode.Right, out expression))
            return true;

        destination = null!;
        expression = null!;
        return false;
    }

    private static bool IsBinaryExpressionSupported(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, BinaryExpressionSyntax expressionNode, out IExpression expression)
    {
        if (IsExpressionSupported(fieldTable, parameterTable, expressionNode.Left, out IExpression Left) &&
            IsExpressionSupported(fieldTable, parameterTable, expressionNode.Right, out IExpression Right))
        {
            string OperatorText;

            if (IsBinaryOperatorSupported(expressionNode.OperatorToken, out OperatorText))
            {
                expression = new BinaryExpression { Left = Left, OperatorText = OperatorText, Right = Right };
                return true;
            }
            else if (IsComparisonOperatorSupported(expressionNode.OperatorToken, out OperatorText))
            {
                expression = new ComparisonExpression { Left = Left, OperatorText = OperatorText, Right = Right };
                return true;
            }
            else if (IsBinaryLogicalOperatorSupported(expressionNode.OperatorToken, out OperatorText))
            {
                expression = new BinaryLogicalExpression { Left = Left, OperatorText = OperatorText, Right = Right };
                return true;
            }
        }

        expression = null!;
        return false;
    }

    private static bool IsBinaryOperatorSupported(SyntaxToken token, out string operatorText)
    {
        operatorText = token.ValueText;

        return token.IsKind(SyntaxKind.PlusToken) ||
               token.IsKind(SyntaxKind.MinusToken)
               ;
    }

    private static bool IsComparisonOperatorSupported(SyntaxToken token, out string operatorText)
    {
        operatorText = token.ValueText;

        return token.IsKind(SyntaxKind.EqualsEqualsToken) ||
               token.IsKind(SyntaxKind.ExclamationEqualsToken) ||
               token.IsKind(SyntaxKind.GreaterThanToken) ||
               token.IsKind(SyntaxKind.GreaterThanEqualsToken) ||
               token.IsKind(SyntaxKind.LessThanToken) ||
               token.IsKind(SyntaxKind.LessThanEqualsToken);
    }

    private static bool IsBinaryLogicalOperatorSupported(SyntaxToken token, out string operatorText)
    {
        operatorText = token.ValueText;

        return token.IsKind(SyntaxKind.BarBarToken) ||
               token.IsKind(SyntaxKind.AmpersandAmpersandToken)
               ;
    }

    private static bool IsStatementOrBlockSupported(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, StatementSyntax node, out List<IStatement> statementList)
    {
        if (node is BlockSyntax Block)
            return IsBlockSupported(fieldTable, parameterTable, Block, out statementList);
        else if (IsStatementSupported(fieldTable, parameterTable, node, out IStatement Statement))
        {
            statementList = new List<IStatement> { Statement };
            return true;
        }

        statementList = null!;
        return false;
    }

    private static bool IsBlockSupported(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, BlockSyntax block, out List<IStatement> statementList)
    {
        statementList = new List<IStatement>();

        foreach (StatementSyntax Item in block.Statements)
            if (!IsStatementSupported(fieldTable, parameterTable, Item, out IStatement Statement))
                return false;
            else
                statementList.Add(Statement);

        return true;
    }

    private static bool IsStatementSupported(Dictionary<FieldName, IField> fieldTable, Dictionary<ParameterName, IParameter> parameterTable, StatementSyntax node, out IStatement statement)
    {
        if (node is ExpressionStatementSyntax ExpressionStatement &&
            ExpressionStatement.Expression is AssignmentExpressionSyntax AssignmentExpression &&
            IsAssignmentExpressionSupported(fieldTable, parameterTable, AssignmentExpression, out IField Destination, out IExpression Expression))
        {
            statement = new AssignmentStatement { Destination = Destination, Expression = Expression };
            return true;
        }
        else if (node is IfStatementSyntax IfStatement &&
            IsExpressionSupported(fieldTable, parameterTable, IfStatement.Condition, out IExpression Condition) &&
            IsStatementOrBlockSupported(fieldTable, parameterTable, IfStatement.Statement, out List<IStatement> WhenTrueStatementList))
        {
            List<IStatement> WhenFalseStatementList = new();
            if (IfStatement.Else is not ElseClauseSyntax ElseClause || IsStatementOrBlockSupported(fieldTable, parameterTable, ElseClause.Statement, out WhenFalseStatementList))
            {
                statement = new ConditionalStatement { Condition = Condition, WhenTrueStatementList = WhenTrueStatementList, WhenFalseStatementList = WhenFalseStatementList };
                return true;
            }
        }
        else if (node is ReturnStatementSyntax ReturnStatement)
        {
            IExpression? ReturnExpression = null;

            if (ReturnStatement.Expression is null || IsExpressionSupported(fieldTable, parameterTable, ReturnStatement.Expression, out ReturnExpression))
            {
                statement = new ReturnStatement { Expression = ReturnExpression };
                return true;
            }
        }

        statement = null!;
        return false;
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

    private static List<IInvariant> ParseInvariants(ClassDeclarationSyntax classDeclaration, Dictionary<FieldName, IField> fieldTable)
    {
        List<IInvariant> InvariantList = new();

        SyntaxToken LastToken = classDeclaration.GetLastToken();
        var Location = LastToken.GetLocation();

        if (LastToken.HasTrailingTrivia)
        {
            SyntaxTriviaList TrailingTrivia = LastToken.TrailingTrivia;
            AddInvariantsInTrivia(InvariantList, fieldTable, TrailingTrivia);
            Location = TrailingTrivia.Last().GetLocation();
        }

        var NextToken = classDeclaration.SyntaxTree.GetRoot().FindToken(Location.SourceSpan.End);

        if (NextToken.HasLeadingTrivia)
            AddInvariantsInTrivia(InvariantList, fieldTable, NextToken.LeadingTrivia);

        return InvariantList;
    }

    private static void AddInvariantsInTrivia(List<IInvariant> invariantList, Dictionary<FieldName, IField> fieldTable, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Invariant}";

                if (Comment.StartsWith(Pattern))
                    AddInvariantsInTrivia(invariantList, fieldTable, Trivia, Comment, Pattern);
            }
    }

    private static void AddInvariantsInTrivia(List<IInvariant> invariantList, Dictionary<FieldName, IField> fieldTable, SyntaxTrivia trivia, string comment, string pattern)
    {
        string Text = comment.Substring(pattern.Length);

        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText($"_ = {Text};", Options);
        var Diagnostics = SyntaxTree.GetDiagnostics();
        List<Diagnostic> ErrorList = Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id != "CS1029").ToList();

        IInvariant NewInvariant;

        if (ErrorList.Count == 0 && IsValidInvariantSyntaxTree(fieldTable, SyntaxTree, out IField Field, out string OperatorText, out int ConstantValue))
        {
            NewInvariant = new Invariant { Text = Text, Field = Field, OperatorText = OperatorText, ConstantValue = ConstantValue };
        }
        else
        {
            Logger.Log($"Bad invariant {Text}");

            Location FullLocation = trivia.GetLocation();
            TextSpan FullSpan = FullLocation.SourceSpan;
            TextSpan InvariantSpan = new TextSpan(FullSpan.Start + pattern.Length, FullSpan.Length - pattern.Length);
            Location Location = Location.Create(FullLocation.SourceTree!, InvariantSpan);

            NewInvariant = new UnsupportedInvariant { Text = Text, Location = Location };
        }

        invariantList.Add(NewInvariant);
    }

    private static bool IsValidInvariantSyntaxTree(Dictionary<FieldName, IField> fieldTable, SyntaxTree syntaxTree, out IField field, out string operatorText, out int constantValue)
    {
        field = null!;
        operatorText = string.Empty;
        constantValue = 0;

        CompilationUnitSyntax Root = syntaxTree.GetCompilationUnitRoot();
        if (Root.AttributeLists.Count > 0 || Root.Usings.Count > 0 || Root.Members.Count != 1)
            return false;

        if (Root.Members[0] is not GlobalStatementSyntax GlobalStatement || 
            GlobalStatement.Statement is not ExpressionStatementSyntax ExpressionStatement ||
            ExpressionStatement.Expression is not AssignmentExpressionSyntax AssignmentExpression)
            return false;

        ExpressionSyntax Expression = AssignmentExpression.Right;
        return IsValidInvariantExpression(fieldTable,  Expression, out field, out operatorText, out constantValue);
    }

    private static bool IsValidInvariantExpression(Dictionary<FieldName, IField> fieldTable, ExpressionSyntax expression, out IField field, out string operatorText, out int constantValue)
    {
        field = null!;
        operatorText = string.Empty;
        constantValue = 0;

        if (expression is not BinaryExpressionSyntax BinaryExpression)
            return false;

        ExpressionSyntax LeftExpression = BinaryExpression.Left;
        SyntaxToken Operator = BinaryExpression.OperatorToken;
        ExpressionSyntax RightExpression = BinaryExpression.Right;

        if (LeftExpression is not IdentifierNameSyntax IdentifierName || 
            !TryFindFieldByName(fieldTable, IdentifierName.Identifier.ValueText, out field))
            return false;

        if (!IsComparisonOperatorSupported(Operator, out operatorText))
            return false;

        if (RightExpression is not LiteralExpressionSyntax LiteralExpression ||
            !LiteralExpression.IsKind(SyntaxKind.NumericLiteralExpression) ||
            !int.TryParse(LiteralExpression.Token.ValueText, out constantValue))
            return false;

        return true;
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
                    Result += @$"  * {Invariant.FieldName} {Invariant.OperatorText} {Invariant.ConstantValue}
";
            }

        return Result;
    }
}
