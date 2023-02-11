namespace ModelAnalyzer;

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
    /// Gets the list of method call statements.
    /// </summary>
    public List<MethodCallStatementEntry> MethodCallStatementList { get; private set; } = new();

    /// <summary>
    /// Gets the list of function call expressions.
    /// </summary>
    public List<FunctionCallExpressionEntry> FunctionCallExpressionList { get; private set; } = new();

    /// <summary>
    /// Gets the list of arithmetic expressions.
    /// </summary>
    public List<ArithmeticExpressionEntry> ArithmeticExpressionList { get; private set; } = new();

    /// <summary>
    /// Parses the class declaration, first phase.
    /// </summary>
    public void ParsePhase1()
    {
        ClassName ClassName = SemanticModel.ClassDeclarationToClassName(ClassDeclaration);
        Debug.Assert(ClassName != ClassName.Empty);

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
        ClassName ClassName = SemanticModel.ClassDeclarationToClassName(ClassDeclaration);
        Debug.Assert(ClassName != ClassName.Empty);

        Log($"Parsing declaration of class '{ClassName}', phase two");

        LocationId.Reset();
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
            MethodCallStatementList = ParsingContext.MethodCallStatementList;
            FunctionCallExpressionList = ParsingContext.FunctionCallExpressionList;
            ArithmeticExpressionList = ParsingContext.ArithmeticExpressionList;
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

    /// <summary>
    /// Parses the text of an assertion.
    /// </summary>
    /// <param name="method">The mthod containing the assertion.</param>
    /// <param name="text">The assertion text.</param>
    /// <param name="callLocation">The assertion location.</param>
    public Expression? ParseAssertionText(Method method, string text, ICallLocation callLocation)
    {
        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText($"_ = {text};", Options);
        CompilationUnitSyntax Root = SyntaxTree.GetCompilationUnitRoot();
        GlobalStatementSyntax GlobalStatement = (GlobalStatementSyntax)Root.Members[0];
        ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)GlobalStatement.Statement;
        AssignmentExpressionSyntax AssignmentExpression = (AssignmentExpressionSyntax)ExpressionStatement.Expression;
        ExpressionSyntax ExpressionNode = AssignmentExpression.Right;
        LocationContext LocationContext = new(ExpressionNode);

        ParsingContext ParsingContext = new()
        {
            ClassDeclarationList = new List<ClassDeclarationSyntax>(),
            ClassName = ClassName.Empty,
            SemanticModel = SemanticModel,
            HostMethod = method,
            LocationContext = LocationContext,
            CallLocation = callLocation,
        };
        Expression? Expression = ParseExpression(ParsingContext, ExpressionNode);

        return Expression;
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

    private bool IsTypeSupported(ParsingContext parsingContext, TypeSyntax? type, out ExpressionType variableType, out ArraySize arraySize)
    {
        arraySize = ArraySize.Invalid;

        if (type is PredefinedTypeSyntax PredefinedType)
            return IsPredefinedTypeSupported(PredefinedType, out variableType);
        else if (type is NullableTypeSyntax NullableType)
            return IsNullableClassTypeKnown(parsingContext, NullableType, out variableType);
        else if (type is IdentifierNameSyntax IdentifierName)
            return IsClassTypeKnown(parsingContext, IdentifierName, isNullable: false, out variableType);
        else if (type is ArrayTypeSyntax ArrayType)
            return IsArrayTypeSupported(parsingContext, ArrayType, out variableType, out arraySize);
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

    private bool IsArrayTypeSupported(ParsingContext parsingContext, ArrayTypeSyntax arrayType, out ExpressionType variableType, out ArraySize arraySize)
    {
        arraySize = ArraySize.Invalid;

        if (arrayType.RankSpecifiers.Count == 1)
        {
            ArrayRankSpecifierSyntax FirstRank = arrayType.RankSpecifiers[0];
            if (FirstRank.Sizes.Count == 1)
            {
                ExpressionSyntax FirstSize = FirstRank.Sizes[0];
                if (FirstSize is OmittedArraySizeExpressionSyntax)
                    arraySize = ArraySize.Unknown;
                else if (FirstSize is LiteralExpressionSyntax LiteralExpression)
                {
                    string LiteralValue = LiteralExpression.Token.Text;
                    if (int.TryParse(LiteralValue, out int IntegerValue))
                        arraySize = new ArraySize() { Size = IntegerValue };
                }

                if (arraySize.IsValid)
                {
                    TypeSyntax ElementType = arrayType.ElementType;
                    if (IsTypeSupported(parsingContext, ElementType, out ExpressionType ArrayElementType, out _))
                    {
                        Debug.Assert(!ArrayElementType.IsArray);

                        variableType = ArrayElementType.ToArrayType();
                        return true;
                    }
                }
            }
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
            Argument? NewArgument = TryParseArgument(parsingContext, InvocationArgument, ref isErrorReported);

            if (NewArgument is not null)
                ArgumentList.Add(NewArgument);
        }

        return ArgumentList;
    }

    private Argument? TryParseArgument(ParsingContext parsingContext, ArgumentSyntax argument, ref bool isErrorReported)
    {
        Argument? NewArgument = null;

        if (argument.NameColon is not null)
            Log("Named argument not supported.");
        else if (!argument.RefKindKeyword.IsKind(SyntaxKind.None))
            Log("ref, out or in arguments not supported.");
        else
        {
            ExpressionSyntax ArgumentExpression = argument.Expression;
            LocationContext LocationContext = new(ArgumentExpression);
            ParsingContext MethodCallParsingContext = parsingContext with { LocationContext = LocationContext, IsExpressionNested = false };

            Expression? Expression = ParseExpression(MethodCallParsingContext, ArgumentExpression);
            if (Expression is not null)
                NewArgument = new() { Expression = Expression, Location = argument.GetLocation() };
            else
                isErrorReported = true;
        }

        return NewArgument;
    }

    private bool TryParsePropertyPath(ParsingContext parsingContext, MemberAccessExpressionSyntax memberAccessExpression, out List<IVariable> variablePath, out string lastName, out Location pathLocation)
    {
        variablePath = new List<IVariable>();

        if (TryParseNamePath(memberAccessExpression, out List<string> NamePath, out pathLocation))
        {
            Debug.Assert(NamePath.Count >= 2);

            string LeftName = NamePath[0];

            if (TryFindVariableByName(parsingContext, LeftName, out IVariable Variable))
            {
                variablePath.Add(Variable);
                string PropertyName = string.Empty;

                for (int i = 0; i + 2 < NamePath.Count; i++)
                    if (TryParseNextVariable(parsingContext, NamePath, i, ref Variable))
                        variablePath.Add(Variable);
                    else
                        break;

                if (variablePath.Count + 1 == NamePath.Count)
                {
                    lastName = NamePath.Last();
                    return true;
                }
                else
                    Log($"Unknown property '{PropertyName}'.");
            }
            else
                Log($"Unknown variable '{LeftName}'.");
        }

        lastName = null!;
        pathLocation = null!;
        return false;
    }

    private bool TryParseNamePath(MemberAccessExpressionSyntax memberAccessExpression, out List<string> namePath, out Location pathLocation)
    {
        namePath = new List<string>();
        MemberAccessExpressionSyntax ObjectExpression = memberAccessExpression;

        while (ObjectExpression.Expression is MemberAccessExpressionSyntax NestedObjectExpression && ObjectExpression.OperatorToken.IsKind(SyntaxKind.DotToken))
        {
            string NextName = ObjectExpression.Name.Identifier.ValueText;
            namePath.Insert(0, NextName);

            ObjectExpression = NestedObjectExpression;
        }

        if (ObjectExpression.Expression is IdentifierNameSyntax ObjectName)
        {
            string LeftName = ObjectName.Identifier.ValueText;
            string RightName = ObjectExpression.Name.Identifier.ValueText;
            namePath.Insert(0, RightName);
            namePath.Insert(0, LeftName);

            pathLocation = memberAccessExpression.GetLocation();
            return true;
        }

        pathLocation = null!;
        return false;
    }

    private bool TryParseNextVariable(ParsingContext parsingContext, List<string> namePath, int index, ref IVariable variable)
    {
        Debug.Assert(index >= 0);
        Debug.Assert(index + 2 < namePath.Count);

        ExpressionType VariableType = variable.Type;

        if (VariableType.IsSimple || VariableType.IsArray)
            return false;

        ClassName ClassName = VariableType.TypeName;
        Dictionary<ClassName, IClassModel> Phase1ClassModelTable = parsingContext.SemanticModel.Phase1ClassModelTable;

        Debug.Assert(Phase1ClassModelTable.ContainsKey(ClassName));

        IClassModel ClassModel = Phase1ClassModelTable[ClassName];
        IList<IProperty> PropertyList = ClassModel.GetProperties();
        string PropertyName = namePath[index + 1];
        bool IsFound = false;

        foreach (IProperty Item in PropertyList)
            if (Item.Name.Text == PropertyName)
            {
                variable = Item;
                IsFound = true;
                break;
            }

        if (!IsFound)
            return false;

        return true;
    }

    private bool TryParseTypeName(ParsingContext parsingContext, MemberAccessExpressionSyntax memberAccessExpression, out ClassModel classModel, out string lastName, out Location pathLocation)
    {
        classModel = null!;

        if (TryParseNamePath(memberAccessExpression, out List<string> NamePath, out pathLocation))
        {
            Debug.Assert(NamePath.Count >= 2);

            if (NamePath.Count == 2)
            {
                string LeftName = NamePath[0];
                ClassName ClassName = ClassName.FromSimpleString(LeftName);

                if (parsingContext.SemanticModel.Phase1ClassModelTable.ContainsKey(ClassName))
                {
                    classModel = (ClassModel)parsingContext.SemanticModel.Phase1ClassModelTable[ClassName];
                    lastName = NamePath.Last();
                    return true;
                }
                else
                    Log($"Unknown class '{LeftName}'.");
            }
        }

        lastName = null!;
        pathLocation = null!;
        return false;
    }

    private bool TryParseLastNameAsProperty(ParsingContext parsingContext, List<IVariable> variablePath, string lastName, out Property property)
    {
        if (GetLastClassModel(parsingContext, variablePath, out ClassModel ClassModel))
            return TryParseLastNameAsProperty(parsingContext, ClassModel, lastName, out property);
        else if (GetLastArrayLength(variablePath, lastName, out property))
            return true;

        property = null!;
        return false;
    }

    private bool TryParseLastNameAsProperty(ParsingContext parsingContext, ClassModel classModel, string lastName, out Property property)
    {
        foreach (KeyValuePair<PropertyName, Property> Entry in classModel.PropertyTable)
            if (Entry.Key.Text == lastName)
            {
                property = Entry.Value;
                return true;
            }

        Log($"Unknown property '{lastName}'.");

        property = null!;
        return false;
    }

    private bool TryParseLastNameAsMethod(ParsingContext parsingContext, List<IVariable> variablePath, string lastName, out ClassName className, out Method method)
    {
        if (GetLastClassModel(parsingContext, variablePath, out ClassModel ClassModel))
        {
            className = ClassModel.ClassName;
            return TryParseLastNameAsMethod(parsingContext, ClassModel, lastName, out method);
        }

        className = null!;
        method = null!;
        return false;
    }

    private bool TryParseLastNameAsMethod(ParsingContext parsingContext, ClassModel classModel, string lastName, out Method method)
    {
        foreach (KeyValuePair<MethodName, Method> Entry in classModel.MethodTable)
            if (Entry.Key.Text == lastName)
            {
                method = Entry.Value;
                return true;
            }

        Log($"Unknown method '{lastName}'.");

        method = null!;
        return false;
    }

    public static bool GetLastClassModel(ParsingContext parsingContext, List<IVariable> variablePath, out ClassModel classModel)
    {
        Debug.Assert(variablePath.Count >= 1);

        IVariable LastVariable = variablePath.Last();
        ExpressionType VariableType = LastVariable.Type;

        if (!VariableType.IsArray && !VariableType.IsSimple)
        {
            classModel = GetClassModel(parsingContext, VariableType.TypeName);
            return true;
        }

        classModel = null!;
        return false;
    }

    public static ClassModel GetClassModel(ParsingContext parsingContext, ClassName className)
    {
        Dictionary<ClassName, IClassModel> Phase1ClassModelTable = parsingContext.SemanticModel.Phase1ClassModelTable;

        Debug.Assert(Phase1ClassModelTable.ContainsKey(className));

        ClassModel ClassModel = (ClassModel)Phase1ClassModelTable[className];
        return ClassModel;
    }

    public static bool GetLastArrayLength(List<IVariable> variablePath, string lastName, out Property property)
    {
        Debug.Assert(variablePath.Count >= 1);

        IVariable LastVariable = variablePath.Last();
        ExpressionType VariableType = LastVariable.Type;

        if (VariableType.IsArray && lastName == "Length")
        {
            property = Property.ArrayLengthProperty;
            return true;
        }

        property = null!;
        return false;
    }

    private bool TryParseElementIndex(ParsingContext parsingContext, SeparatedSyntaxList<ArgumentSyntax> arguments, out Expression elementIndex, ref bool isErrorReported)
    {
        if (arguments.Count == 1)
        {
            Argument? NewArgument = TryParseArgument(parsingContext, arguments[0], ref isErrorReported);
            if (NewArgument is not null)
            {
                if (NewArgument.Expression is LiteralIntegerValueExpression ArgumentIntegerValue)
                {
                    elementIndex = ArgumentIntegerValue;
                    return true;
                }
                else if (NewArgument.Expression is VariableValueExpression ArgumentVariableValue)
                {
                    if (ArgumentVariableValue.VariablePath.Count == 1)
                    {
                        elementIndex = ArgumentVariableValue;
                        return true;
                    }
                }
            }
        }

        elementIndex = null!;
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
