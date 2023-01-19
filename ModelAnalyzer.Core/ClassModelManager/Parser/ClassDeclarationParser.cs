namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using AnalysisLogger;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassDeclarationParser"/> class.
    /// </summary>
    /// <param name="classDeclarationList">The list of class declarations containing <paramref name="classDeclaration"/>.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="semanticModel">The semantic model.</param>
    public ClassDeclarationParser(List<ClassDeclarationSyntax> classDeclarationList, ClassDeclarationSyntax classDeclaration, IModel semanticModel)
    {
        ClassDeclarationList = classDeclarationList;
        ClassDeclaration = classDeclaration;
        SemanticModel = semanticModel;
    }

    /// <summary>
    /// Gets the list of class declarations containing <see cref="ClassDeclaration"/>.
    /// </summary>
    public List<ClassDeclarationSyntax> ClassDeclarationList { get; }

    /// <summary>
    /// Gets the class declaration to be parsed.
    /// </summary>
    public ClassDeclarationSyntax ClassDeclaration { get; }

    /// <summary>
    /// Gets the semantic model.
    /// </summary>
    public IModel SemanticModel { get; }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    public IAnalysisLogger Logger { get; init; } = new NullLogger();

    /// <summary>
    /// Gets the property table.
    /// </summary>
    public ReadOnlyPropertyTable PropertyTable { get; private set; } = ReadOnlyPropertyTable.Empty;

    /// <summary>
    /// Gets the field table.
    /// </summary>
    public ReadOnlyFieldTable FieldTable { get; private set; } = ReadOnlyFieldTable.Empty;

    /// <summary>
    /// Gets the method table.
    /// </summary>
    public ReadOnlyMethodTable MethodTable { get; private set; } = ReadOnlyMethodTable.Empty;

    /// <summary>
    /// Gets the list of invariants.
    /// </summary>
    public IReadOnlyList<Invariant> InvariantList { get; private set; } = new List<Invariant>().AsReadOnly();

    /// <summary>
    /// Gets unsupported elements.
    /// </summary>
    public Unsupported Unsupported { get; private set; } = new();

    /// <summary>
    /// Parses the class declaration, first phase.
    /// </summary>
    public void ParsePhase1()
    {
        string ClassName = ClassDeclaration.Identifier.ValueText;
        Debug.Assert(ClassName != string.Empty);

        Log($"Parsing declaration of class '{ClassName}', phase one");

        ParsingContext ParsingContext = new() { ClassDeclarationList = ClassDeclarationList, ClassName = ClassName, SemanticModel = SemanticModel };

        if (IsClassDeclarationSupported(ClassDeclaration))
        {
            ParsingContext = ParsingContext with { PropertyTable = ParseProperties(ParsingContext, ClassDeclaration) };
            ParsingContext = ParsingContext with { FieldTable = ParseFields(ParsingContext, ClassDeclaration) };
            ParsingContext = ParsingContext with { MethodTable = ParseMethods(ParsingContext, ClassDeclaration) };
            ParsingContext = ParsingContext with { IsMethodParsingFirstPassDone = true };

            PropertyTable = ParsingContext.PropertyTable.AsReadOnly();
            FieldTable = ParsingContext.FieldTable.AsReadOnly();
            MethodTable = ParsingContext.MethodTable.AsReadOnly();
        }
        else
        {
            ParsingContext.Unsupported.InvalidDeclaration = true;
            PropertyTable = ReadOnlyPropertyTable.Empty;
            FieldTable = ReadOnlyFieldTable.Empty;
            MethodTable = ReadOnlyMethodTable.Empty;
        }

        Unsupported = ParsingContext.Unsupported;
    }

    /// <summary>
    /// Parses the class declaration, second phase.
    /// </summary>
    public void ParsePhase2()
    {
        string ClassName = ClassDeclaration.Identifier.ValueText;
        Debug.Assert(ClassName != string.Empty);

        Log($"Parsing declaration of class '{ClassName}', phase two");

        ParsingContext ParsingContext = new() { ClassDeclarationList = ClassDeclarationList, ClassName = ClassName, SemanticModel = SemanticModel };

        if (IsClassDeclarationSupported(ClassDeclaration))
        {
            ParsingContext.Unsupported.HasUnsupporteMember = CheckUnsupportedMembers(ClassDeclaration);

            ParsingContext = ParsingContext with { PropertyTable = ParseProperties(ParsingContext, ClassDeclaration) };
            ParsingContext = ParsingContext with { FieldTable = ParseFields(ParsingContext, ClassDeclaration) };
            ParsingContext = ParsingContext with { MethodTable = ParseMethods(ParsingContext, ClassDeclaration) };
            ParsingContext = ParsingContext with { IsMethodParsingFirstPassDone = true };
            ParsingContext = ParsingContext with { MethodTable = ParseMethods(ParsingContext, ClassDeclaration) };
            ParsingContext = ParsingContext with { IsMethodParsingComplete = true };
            ParsingContext = ParsingContext with { InvariantList = ParseInvariants(ParsingContext, ClassDeclaration) };
            ReportInvalidCalls(ParsingContext);

            PropertyTable = ParsingContext.PropertyTable.AsReadOnly();
            FieldTable = ParsingContext.FieldTable.AsReadOnly();
            MethodTable = ParsingContext.MethodTable.AsReadOnly();
            InvariantList = ParsingContext.InvariantList.AsReadOnly();
        }
        else
        {
            ParsingContext.Unsupported.InvalidDeclaration = true;
            PropertyTable = ReadOnlyPropertyTable.Empty;
            FieldTable = ReadOnlyFieldTable.Empty;
            MethodTable = ReadOnlyMethodTable.Empty;
            InvariantList = new List<Invariant>().AsReadOnly();
        }

        Unsupported = ParsingContext.Unsupported;
    }

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

        if (SemanticModel.HasBaseType(classDeclaration))
        {
            LogWarning("Unsupported class base.");

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
            if (Member is not PropertyDeclarationSyntax && Member is not FieldDeclarationSyntax && Member is not MethodDeclarationSyntax)
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

    private bool IsTypeSupported(ParsingContext parsingContext, TypeSyntax? type, out ExpressionType variableType)
    {
        if (type is PredefinedTypeSyntax PredefinedType)
            return IsPredefinedTypeSupported(PredefinedType, out variableType);
        else if (type is NullableTypeSyntax NullableType)
            return IsNullableClassTypeKnown(parsingContext, NullableType, out variableType);
        else if (type is IdentifierNameSyntax IdentifierName)
            return IsClassTypeKnown(parsingContext, IdentifierName, isNullable: false, out variableType);
        else
        {
            variableType = ExpressionType.Other;
            return false;
        }
    }

    private bool IsPredefinedTypeSupported(PredefinedTypeSyntax predefinedType, out ExpressionType variableType)
    {
        SyntaxKind TypeKind = predefinedType.Keyword.Kind();
        Dictionary<SyntaxKind, ExpressionType> SupportedSyntaxKind = new()
        {
            { SyntaxKind.VoidKeyword, ExpressionType.Void },
            { SyntaxKind.BoolKeyword, ExpressionType.Boolean },
            { SyntaxKind.IntKeyword, ExpressionType.Integer },
            { SyntaxKind.DoubleKeyword, ExpressionType.FloatingPoint },
        };

        if (SupportedSyntaxKind.ContainsKey(TypeKind))
        {
            variableType = SupportedSyntaxKind[TypeKind];
            return true;
        }

        variableType = ExpressionType.Other;
        return false;
    }

    private bool IsNullableClassTypeKnown(ParsingContext parsingContext, NullableTypeSyntax nullableType, out ExpressionType variableType)
    {
        if (nullableType.ElementType is IdentifierNameSyntax IdentifierName)
            return IsClassTypeKnown(parsingContext, IdentifierName, isNullable: true, out variableType);

        variableType = ExpressionType.Other;
        return false;
    }

    private bool IsClassTypeKnown(ParsingContext parsingContext, IdentifierNameSyntax identifierName, bool isNullable, out ExpressionType variableType)
    {
        IModel SemanticModel = parsingContext.SemanticModel;
        if (SemanticModel.GetClassType(identifierName, parsingContext.ClassDeclarationList, isNullable, out ExpressionType ClassType))
        {
            variableType = ClassType;
            return true;
        }

        variableType = ExpressionType.Other;
        return false;
    }

    private bool TryParseAssertionTextInTrivia(string text, out SyntaxTree syntaxTree, out int offset)
    {
        const string AssignmentAssertionText = "_ = ";
        offset = AssignmentAssertionText.Length;

        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        syntaxTree = CSharpSyntaxTree.ParseText($"{AssignmentAssertionText}{text};", Options);
        var Diagnostics = syntaxTree.GetDiagnostics();
        List<Diagnostic> ErrorList = Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();
        Log($"Parsed: '{text}' ErrorCount={ErrorList.Count}");

        if (ErrorList.Count > 0)
        {
            Log($"Expression '{text}' contains errors.");

            return false;
        }

        return true;
    }

    private bool IsValidAssertionSyntaxTree(ParsingContext parsingContext, SyntaxTree syntaxTree, out Expression booleanExpression, out bool isErrorReported)
    {
        bool IsAssertionSupported = true;

        CompilationUnitSyntax Root = syntaxTree.GetCompilationUnitRoot();
        Debug.Assert(Root.AttributeLists.Count == 0, "The root begins with an assignment and no attributes.");
        Debug.Assert(Root.Usings.Count == 0, "The root begins with an assignment and no usings.");

        if (Root.Members.Count != 1)
        {
            Log($"There can be only one expression in an assertion.");
            IsAssertionSupported = false;
        }

        GlobalStatementSyntax GlobalStatement = (GlobalStatementSyntax)Root.Members[0];
        ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)GlobalStatement.Statement;
        AssignmentExpressionSyntax AssignmentExpression = (AssignmentExpressionSyntax)ExpressionStatement.Expression;
        ExpressionSyntax Expression = AssignmentExpression.Right;

        if (!IsValidAssertionExpression(parsingContext, Expression, out booleanExpression, out isErrorReported))
            IsAssertionSupported = false;
        else
            isErrorReported = false;

        return IsAssertionSupported;
    }

    private bool IsValidAssertionExpression(ParsingContext parsingContext, ExpressionSyntax expressionNode, out Expression booleanExpression, out bool isErrorReported)
    {
        booleanExpression = null!;
        isErrorReported = false;

        Expression? Expression = ParseExpression(parsingContext, expressionNode);
        if (Expression is not null)
        {
            if (Expression.GetExpressionType() == ExpressionType.Boolean)
            {
                booleanExpression = Expression;
                return true;
            }
            else
                Log($"Boolean expression expected.");
        }
        else
            isErrorReported = true;

        return false;
    }

    private bool TryFindVariableByName(ParsingContext parsingContext, string variableName, out IVariable variable)
    {
        if (TryFindPropertyByName(parsingContext, variableName, out IProperty Property))
        {
            variable = Property;
            return true;
        }

        if (parsingContext.IsFieldAllowed && TryFindFieldByName(parsingContext, variableName, out IField Field))
        {
            variable = Field;
            return true;
        }

        if (TryFindParameterByName(parsingContext, variableName, out IParameter Parameter))
        {
            variable = Parameter;
            return true;
        }

        if (parsingContext.IsLocalAllowed && TryFindLocalByName(parsingContext, variableName, out ILocal Local))
        {
            variable = Local;
            return true;
        }

        if (parsingContext.HostMethod is Method HostMethod && HostMethod.ResultLocal is Local ResultLocal && variableName == Ensure.ResultKeyword)
        {
            variable = ResultLocal;
            return true;
        }

        variable = null!;
        return false;
    }

    private List<Argument> TryParseArgumentList(ParsingContext parsingContext, ArgumentListSyntax argumentList, ref bool isErrorReported)
    {
        SeparatedSyntaxList<ArgumentSyntax> InvocationArgumentList = argumentList.Arguments;
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
                else
                    isErrorReported = true;
            }
        }

        return ArgumentList;
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
