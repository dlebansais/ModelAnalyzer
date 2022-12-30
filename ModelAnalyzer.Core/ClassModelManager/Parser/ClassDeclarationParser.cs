﻿namespace ModelAnalyzer;

using System.Collections.Generic;
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
            FieldTable = ReadOnlyFieldTable.Empty;
            MethodTable = ReadOnlyMethodTable.Empty;
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
    public ReadOnlyFieldTable FieldTable { get; private set; } = ReadOnlyFieldTable.Empty;

    /// <summary>
    /// Gets the method table.
    /// </summary>
    public ReadOnlyMethodTable MethodTable { get; private set; } = ReadOnlyMethodTable.Empty;

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

    private bool IsTypeSupported(TypeSyntax? type, out ExpressionType variableType)
    {
        variableType = ExpressionType.Other;

        if (type is not PredefinedTypeSyntax PredefinedType)
            return false;

        SyntaxKind TypeKind = PredefinedType.Keyword.Kind();
        Dictionary<SyntaxKind, ExpressionType> SupportedSyntaxKind = new()
        {
            { SyntaxKind.VoidKeyword, ExpressionType.Void },
            { SyntaxKind.BoolKeyword, ExpressionType.Boolean },
            { SyntaxKind.IntKeyword, ExpressionType.Integer },
            { SyntaxKind.DoubleKeyword, ExpressionType.FloatingPoint },
        };

        if (!SupportedSyntaxKind.ContainsKey(TypeKind))
            return false;

        variableType = SupportedSyntaxKind[TypeKind];
        return true;
    }

    private bool TryParseAssertionTextInTrivia(string text, out SyntaxTree syntaxTree, out int offset)
    {
        const string AssignmentAssertionText = "_ = ";
        offset = AssignmentAssertionText.Length;

        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        syntaxTree = CSharpSyntaxTree.ParseText($"{AssignmentAssertionText}{text};", Options);
        var Diagnostics = syntaxTree.GetDiagnostics();
        List<Diagnostic> ErrorList = Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id != "CS1029").ToList();
        Log($"Parsed: '{text}' ErrorCount={ErrorList.Count}");

        if (ErrorList.Count > 0)
        {
            Log($"Expression '{text}' contains errors.");

            return false;
        }

        return true;
    }

    private bool IsValidAssertionSyntaxTree(ReadOnlyFieldTable fieldTable, Method? hostMethod, Field? resultField, Unsupported unsupported, LocationContext locationContext, SyntaxTree syntaxTree, out Expression booleanExpression, out bool isErrorReported)
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

        if (!IsValidAssertionExpression(fieldTable, hostMethod, resultField, unsupported, locationContext, Expression, out booleanExpression, out isErrorReported))
            IsAssertionSupported = false;
        else
            isErrorReported = false;

        return IsAssertionSupported;
    }

    private bool IsValidAssertionExpression(ReadOnlyFieldTable fieldTable, Method? hostMethod, Field? resultField, Unsupported unsupported, LocationContext locationContext, ExpressionSyntax expressionNode, out Expression booleanExpression, out bool isErrorReported)
    {
        booleanExpression = null!;
        isErrorReported = false;

        Expression? Expression = ParseExpression(fieldTable, hostMethod, resultField, unsupported, locationContext, expressionNode, isNested: false);
        if (Expression is not null)
        {
            if (Expression.GetExpressionType(fieldTable, hostMethod, resultField) == ExpressionType.Boolean)
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

    private bool TryFindVariableByName(ReadOnlyFieldTable fieldTable, Method? hostMethod, Field? resultField, string variableName, out IVariable variable)
    {
        if (TryFindFieldByName(fieldTable, variableName, out Field Field))
        {
            variable = Field;
            return true;
        }

        if (hostMethod is not null)
        {
            if (TryFindParameterByName(hostMethod.ParameterTable, variableName, out IParameter Parameter))
            {
                variable = Parameter;
                return true;
            }
            else
            {
            }
        }
        else
        {
        }

        if (resultField is not null && resultField.Name.Text == variableName)
        {
            variable = resultField;
            return true;
        }

        variable = null!;
        return false;
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
