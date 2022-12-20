namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AnalysisLogger;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassDeclarationParser"/> class.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
    public ClassDeclarationParser(ClassDeclarationSyntax classDeclaration)
    {
        ClassDeclaration = classDeclaration;
    }

    /// <summary>
    /// Parses the class declaration.
    /// </summary>
    public void Parse()
    {
        string ClassName = ClassDeclaration.Identifier.ValueText;
        Debug.Assert(ClassName != string.Empty);

        Log($"Parsing declaration of class '{ClassName}'");

        Unsupported = new Unsupported();

        if (IsClassDeclarationSupported(ClassDeclaration))
        {
            Unsupported.HasUnsupporteMember = CheckUnsupportedMembers(ClassDeclaration);
            FieldTable = ParseFields(ClassDeclaration, Unsupported);
            MethodTable = ParseMethods(ClassDeclaration, FieldTable, Unsupported);
            InvariantList = ParseInvariants(ClassDeclaration, FieldTable, Unsupported);
        }
        else
        {
            Unsupported.InvalidDeclaration = true;
            FieldTable = new FieldTable();
            MethodTable = new MethodTable();
            InvariantList = new List<Invariant>();
        }
    }

    /// <summary>
    /// Gets the class declaration to be parsed.
    /// </summary>
    public ClassDeclarationSyntax ClassDeclaration { get; }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    public IAnalysisLogger Logger { get; init; } = new NullLogger();

    /// <summary>
    /// Gets the field table.
    /// </summary>
    public FieldTable FieldTable { get; private set; } = new();

    /// <summary>
    /// Gets the method table.
    /// </summary>
    public MethodTable MethodTable { get; private set; } = new();

    /// <summary>
    /// Gets the list of invariants.
    /// </summary>
    public List<Invariant> InvariantList { get; private set; } = new();

    /// <summary>
    /// Gets unsupported elements.
    /// </summary>
    public Unsupported Unsupported { get; private set; } = new();

    private bool IsClassDeclarationSupported(ClassDeclarationSyntax classDeclaration)
    {
        bool IsSupported = true;

        if (classDeclaration.AttributeLists.Count > 0)
        {
            LogWarning($"Unsupported {classDeclaration.AttributeLists.Count} class attribute(s).");

            IsSupported = false;
        }

        foreach (SyntaxToken Modifier in classDeclaration.Modifiers)
            if (!Modifier.IsKind(SyntaxKind.PrivateKeyword) && !Modifier.IsKind(SyntaxKind.PublicKeyword) && !Modifier.IsKind(SyntaxKind.InternalKeyword) && !Modifier.IsKind(SyntaxKind.PartialKeyword))
            {
                LogWarning($"Unsupported '{Modifier.ValueText}' class modifier.");

                IsSupported = false;
            }

        if (classDeclaration.BaseList is BaseListSyntax BaseList && BaseList.Types.Count > 0)
        {
            LogWarning("Unsupported class base list.");

            IsSupported = false;
        }

        if (classDeclaration.TypeParameterList is TypeParameterListSyntax TypeParameterList && TypeParameterList.Parameters.Count > 0)
        {
            LogWarning("Unsupported class type parameter(s).");

            IsSupported = false;
        }

        if (classDeclaration.ConstraintClauses.Count > 0)
        {
            LogWarning("Unsupported class constraint(s).");

            IsSupported = false;
        }

        return IsSupported;
    }

    private bool CheckUnsupportedMembers(ClassDeclarationSyntax classDeclaration)
    {
        bool HasUnsupportedNode = false;

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is not FieldDeclarationSyntax && Member is not MethodDeclarationSyntax)
            {
                LogWarning($"Class member type not supported: {Member.GetType().Name}.");

                HasUnsupportedNode = true;
            }

        return HasUnsupportedNode;
    }

    private bool TryFindLeadingTrivia(MemberDeclarationSyntax member, out SyntaxTriviaList triviaList)
    {
        if (member.HasLeadingTrivia)
        {
            triviaList = member.GetLeadingTrivia();
            return true;
        }

        triviaList = default;
        return false;
    }

    private bool TryFindTrailingTrivia(MemberDeclarationSyntax member, out SyntaxTriviaList triviaList)
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

    private bool IsTypeSupported(TypeSyntax? type, out bool isVoid)
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

    private bool TryParseAssertionInTrivia(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, string text, out Expression booleanExpression)
    {
        booleanExpression = null!;

        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText($"_ = {text};", Options);
        var Diagnostics = SyntaxTree.GetDiagnostics();
        List<Diagnostic> ErrorList = Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id != "CS1029").ToList();
        Log($"Parsed: '{text}' ErrorCount={ErrorList.Count}");

        if (ErrorList.Count > 0)
        {
            Log($"Expression '{text}' contains an error.");
            return false;
        }
        else
            return IsValidAssertionSyntaxTree(fieldTable, parameterTable, unsupported, SyntaxTree, out booleanExpression);
    }

    private bool IsValidAssertionSyntaxTree(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, SyntaxTree syntaxTree, out Expression booleanExpression)
    {
        bool IsInvariantSupported = true;

        CompilationUnitSyntax Root = syntaxTree.GetCompilationUnitRoot();
        Debug.Assert(Root.AttributeLists.Count == 0, "The root begins with an assignment and no attributes.");
        Debug.Assert(Root.Usings.Count == 0, "The root begins with an assignment and no usings.");

        if (Root.Members.Count != 1)
        {
            Log($"There can be only one expression in an assertion.");
            IsInvariantSupported = false;
        }

        GlobalStatementSyntax GlobalStatement = (GlobalStatementSyntax)Root.Members[0];
        ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)GlobalStatement.Statement;
        AssignmentExpressionSyntax AssignmentExpression = (AssignmentExpressionSyntax)ExpressionStatement.Expression;
        ExpressionSyntax Expression = AssignmentExpression.Right;

        if (!IsValidAssertionExpression(fieldTable, parameterTable, unsupported, Expression, out booleanExpression))
            IsInvariantSupported = false;

        return IsInvariantSupported;
    }

    private bool IsValidAssertionExpression(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, ExpressionSyntax expressionNode, out Expression booleanExpression)
    {
        booleanExpression = null!;

        Expression? Expression = ParseExpression(fieldTable, parameterTable, unsupported, expressionNode, isNested: false);
        if (Expression is not null)
        {
            if (IsBooleanExpression(Expression))
            {
                booleanExpression = Expression;
                return true;
            }
            else
                Log($"Boolean expression expected.");
        }

        return false;
    }

    private bool IsBooleanExpression(Expression expression)
    {
        return expression is BinaryLogicalExpression || expression is UnaryLogicalExpression || expression is ComparisonExpression || expression is LiteralBoolValueExpression;
    }

    private bool TryFindVariableByName(FieldTable fieldTable, ParameterTable parameterTable, string variableName, out IVariable variable)
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

    private Location GetLocationInComment(SyntaxTrivia trivia, string pattern)
    {
        Location FullLocation = trivia.GetLocation();
        TextSpan FullSpan = FullLocation.SourceSpan;
        TextSpan InvariantSpan = new TextSpan(FullSpan.Start + pattern.Length, FullSpan.Length - pattern.Length);
        Location Location = Location.Create(FullLocation.SourceTree!, InvariantSpan);

        return Location;
    }

    private void Log(string message)
    {
        Logger.Log(message);
    }

    private void LogWarning(string message)
    {
        Logger.Log(LogLevel.Warning, message);
    }
}
