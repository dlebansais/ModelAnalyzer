namespace DemoAnalyzer;

using System.Collections.Generic;
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
        string ClassName = classDeclaration.Identifier.ValueText;
        Unsupported = new Unsupported();

        Log($"Parsing declaration of class '{ClassName}'");

        if (ClassName == string.Empty || !IsClassDeclarationSupported(classDeclaration))
        {
            Unsupported.InvalidDeclaration = true;
            FieldTable = new FieldTable();
            MethodTable = new MethodTable();
            InvariantList = new List<IInvariant>();
        }
        else
        {
            Unsupported.HasUnsupporteMember = CheckUnsupportedMembers(classDeclaration);
            FieldTable = ParseFields(classDeclaration, Unsupported);
            MethodTable = ParseMethods(classDeclaration, FieldTable, Unsupported);
            InvariantList = ParseInvariants(classDeclaration, FieldTable, Unsupported);
        }
    }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    public IAnalysisLogger Logger { get; init; } = new NullLogger();

    /// <summary>
    /// Gets the field table.
    /// </summary>
    public FieldTable FieldTable { get; }

    /// <summary>
    /// Gets the method table.
    /// </summary>
    public MethodTable MethodTable { get; }

    /// <summary>
    /// Gets the list of invariants.
    /// </summary>
    public List<IInvariant> InvariantList { get; }

    /// <summary>
    /// Gets unsupported elements.
    /// </summary>
    public Unsupported Unsupported { get; }

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

    private bool TryParseAssertionInTrivia(FieldTable fieldTable, ParameterTable parameterTable, Unsupported unsupported, string text, out IExpression booleanExpression)
    {
        booleanExpression = null!;

        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText($"_ = {text};", Options);
        var Diagnostics = SyntaxTree.GetDiagnostics();
        List<Diagnostic> ErrorList = Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id != "CS1029").ToList();

        return ErrorList.Count == 0 && IsValidAssertionSyntaxTree(fieldTable, parameterTable, unsupported, SyntaxTree, out booleanExpression);
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
