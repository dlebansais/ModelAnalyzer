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
    private MethodTable ParseMethods(ParsingContext parsingContext, ClassDeclarationSyntax classDeclaration)
    {
        MethodTable MethodTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
        {
            SyntaxTriviaList TriviaList;

            if (parsingContext.IsMethodParsingFirstPassDone && TryFindLeadingTrivia(Member, out TriviaList))
                ReportUnsupportedRequires(parsingContext, TriviaList);

            if (Member is MethodDeclarationSyntax MethodDeclaration)
                AddMethod(parsingContext, MethodTable, MethodDeclaration);
            else if (parsingContext.IsMethodParsingFirstPassDone && TryFindTrailingTrivia(Member, out TriviaList))
                ReportUnsupportedEnsures(parsingContext, TriviaList);
        }

        return MethodTable;
    }

    private void AddMethod(ParsingContext parsingContext, MethodTable methodTable, MethodDeclarationSyntax methodDeclaration)
    {
        string Text = methodDeclaration.Identifier.ValueText;
        MethodName MethodName = new() { Text = Text };

        // Ignore duplicate names, the compiler will catch them.
        if (!methodTable.ContainsItem(MethodName))
        {
            if (IsMethodDeclarationValid(methodDeclaration, out AccessModifier AccessModifier, out ExpressionType ReturnType))
            {
                Method TemporaryMethod;
                ParsingContext MethodParsingContext;

                TemporaryMethod = new Method
                {
                    Name = MethodName,
                    AccessModifier = AccessModifier,
                    ParameterTable = ReadOnlyParameterTable.Empty,
                    ReturnType = ReturnType,
                    RequireList = new List<Require>(),
                    LocalTable = ReadOnlyLocalTable.Empty,
                    StatementList = new List<Statement>(),
                    EnsureList = new List<Ensure>(),
                };
                MethodParsingContext = parsingContext with { HostMethod = TemporaryMethod };

                ReadOnlyParameterTable ParameterTable = ParseParameters(MethodParsingContext, methodDeclaration).AsReadOnly();

                List<Require> RequireList;
                ReadOnlyLocalTable LocalTable;
                List<Statement> StatementList;
                List<Ensure> EnsureList;

                if (parsingContext.IsMethodParsingFirstPassDone)
                {
                    TemporaryMethod = new Method
                    {
                        Name = MethodName,
                        AccessModifier = AccessModifier,
                        ParameterTable = ParameterTable,
                        ReturnType = ReturnType,
                        RequireList = new List<Require>(),
                        LocalTable = ReadOnlyLocalTable.Empty,
                        StatementList = new List<Statement>(),
                        EnsureList = new List<Ensure>(),
                    };
                    MethodParsingContext = MethodParsingContext with { HostMethod = TemporaryMethod };

                    RequireList = ParseRequires(MethodParsingContext, methodDeclaration);

                    LocalTable = ParseLocals(MethodParsingContext, methodDeclaration).AsReadOnly();

                    TemporaryMethod = new Method
                    {
                        Name = MethodName,
                        AccessModifier = AccessModifier,
                        ParameterTable = ParameterTable,
                        ReturnType = ReturnType,
                        RequireList = RequireList,
                        LocalTable = LocalTable,
                        StatementList = new List<Statement>(),
                        EnsureList = new List<Ensure>(),
                    };
                    MethodParsingContext = MethodParsingContext with { HostMethod = TemporaryMethod, IsLocalAllowed = true };

                    StatementList = ParseStatements(MethodParsingContext, methodDeclaration);

                    TemporaryMethod = new Method
                    {
                        Name = MethodName,
                        AccessModifier = AccessModifier,
                        ParameterTable = ParameterTable,
                        ReturnType = ReturnType,
                        RequireList = RequireList,
                        LocalTable = LocalTable,
                        StatementList = StatementList,
                        EnsureList = new List<Ensure>(),
                    };
                    Local? ResultLocal = ReturnType != ExpressionType.Void ? FindOrCreateResultLocal(LocalTable, ReturnType) : null;
                    MethodParsingContext = MethodParsingContext with { HostMethod = TemporaryMethod, IsLocalAllowed = false, ResultLocal = ResultLocal };

                    EnsureList = ParseEnsures(MethodParsingContext, methodDeclaration);
                }
                else
                {
                    RequireList = new List<Require>();
                    LocalTable = ReadOnlyLocalTable.Empty;
                    StatementList = new List<Statement>();
                    EnsureList = new List<Ensure>();
                }

                Method NewMethod = new Method
                {
                    Name = MethodName,
                    AccessModifier = AccessModifier,
                    ParameterTable = ParameterTable,
                    ReturnType = ReturnType,
                    RequireList = RequireList,
                    LocalTable = LocalTable,
                    StatementList = StatementList,
                    EnsureList = EnsureList,
                };

                methodTable.AddItem(NewMethod);
            }
            else if (parsingContext.IsMethodParsingFirstPassDone)
            {
                if (TryFindTrailingTrivia(methodDeclaration, out SyntaxTriviaList TriviaList))
                    ReportUnsupportedEnsures(parsingContext, TriviaList);

                Location Location = methodDeclaration.Identifier.GetLocation();
                parsingContext.Unsupported.AddUnsupportedMethod(Location);
            }
        }
    }

    private bool IsMethodDeclarationValid(MethodDeclarationSyntax methodDeclaration, out AccessModifier accessModifier, out ExpressionType returnType)
    {
        bool IsMethodSupported = true;

        if (methodDeclaration.AttributeLists.Count > 0)
        {
            LogWarning($"Unsupported {methodDeclaration.AttributeLists.Count} method attribute(s).");

            IsMethodSupported = false;
        }

        if (!IsTypeSupported(methodDeclaration.ReturnType, out returnType))
        {
            LogWarning($"Unsupported method return type.");

            IsMethodSupported = false;
        }

        accessModifier = AccessModifier.Private;
        Dictionary<SyntaxKind, AccessModifier> AccessModifierTable = new()
        {
            { SyntaxKind.PrivateKeyword, AccessModifier.Private },
            { SyntaxKind.PublicKeyword, AccessModifier.Public },
            { SyntaxKind.InternalKeyword, AccessModifier.Public },
        };

        foreach (SyntaxToken Modifier in methodDeclaration.Modifiers)
        {
            SyntaxKind ModifierKind = Modifier.Kind();

            if (AccessModifierTable.ContainsKey(ModifierKind))
                accessModifier = AccessModifierTable[ModifierKind];
            else
            {
                LogWarning($"Unsupported '{Modifier.ValueText}' method modifier.");

                IsMethodSupported = false;
            }
        }

        return IsMethodSupported;
    }

    private void ReportInvalidMethodCalls(ParsingContext parsingContext)
    {
        List<Method> VisitedMethodList = new();

        foreach (KeyValuePair<MethodName, Method> Entry in parsingContext.MethodTable)
        {
            ParsingContext MethodParsingContext = parsingContext with { HostMethod = Entry.Value };
            ReportInvalidMethodCalls(MethodParsingContext, VisitedMethodList);
        }
    }

    private void ReportInvalidMethodCalls(ParsingContext parsingContext, List<Method> visitedMethodList)
    {
        List<Statement> StatementList = parsingContext.HostMethod!.StatementList;
        int i = 0;

        while (i < StatementList.Count)
        {
            if (StatementList[i] is MethodCallStatement MethodCall)
            {
                if (!IsValidMethodCall(parsingContext, visitedMethodList, MethodCall, out Location Location))
                {
                    parsingContext.Unsupported.AddUnsupportedStatement(Location);
                    StatementList.RemoveAt(i);
                }
                else
                    i++;
            }
            else
                i++;
        }
    }

    private bool IsValidMethodCall(ParsingContext parsingContext, List<Method> visitedMethodList, MethodCallStatement methodCall, out Location location)
    {
        MethodTable MethodTable = parsingContext.MethodTable;
        Method HostMethod = parsingContext.HostMethod!;

        location = methodCall.NameLocation;

        if (methodCall.MethodName == HostMethod.Name)
            return false;

        Method? CalledMethod = null;

        foreach (var Entry in MethodTable)
            if (Entry.Key == methodCall.MethodName)
            {
                CalledMethod = Entry.Value;
                break;
            }

        if (CalledMethod is null)
            return false;

        if (visitedMethodList.Contains(CalledMethod))
            return false;

        int Count = methodCall.ArgumentList.Count;

        if (Count != CalledMethod.ParameterTable.Count)
            return false;

        int Index = 0;
        bool AllArgumentsValid = true;

        foreach (KeyValuePair<ParameterName, Parameter> Entry in CalledMethod.ParameterTable)
        {
            Argument Argument = methodCall.ArgumentList[Index++];
            Parameter Parameter = Entry.Value;

            if (!IsValidMethodCallArgument(parsingContext, Argument, Parameter))
            {
                location = Argument.Location;
                AllArgumentsValid = false;
                break;
            }
        }

        if (!AllArgumentsValid)
            return false;

        List<Method> NewVisitedMethodList = new();
        NewVisitedMethodList.AddRange(visitedMethodList);
        NewVisitedMethodList.Add(CalledMethod);

        ParsingContext CalledMethodParsingContext = parsingContext with { HostMethod = CalledMethod };
        ReportInvalidMethodCalls(CalledMethodParsingContext, NewVisitedMethodList);

        return true;
    }

    private bool IsValidMethodCallArgument(ParsingContext parsingContext, Argument argument, Parameter parameter)
    {
        Expression ArgumentExpression = (Expression)argument.Expression;
        ExpressionType ArgumentType = ArgumentExpression.GetExpressionType(parsingContext);
        ExpressionType ParameterType = parameter.Type;

        if (!IsSourceAndDestinationTypeCompatible(ParameterType, ArgumentType))
            return false;

        return true;
    }

    private Local FindOrCreateResultLocal(ReadOnlyLocalTable localTable, ExpressionType returnType)
    {
        Debug.Assert(returnType != ExpressionType.Other);
        Debug.Assert(returnType != ExpressionType.Void);

        LocalName ResultName = new LocalName() { Text = Ensure.ResultKeyword };

        foreach (KeyValuePair<LocalName, Local> Entry in localTable)
            if (Entry.Key == ResultName)
                return Entry.Value;

        Local ResultLocal = new Local() { Name = ResultName, Type = returnType, Initializer = null };

        return ResultLocal;
    }
}
