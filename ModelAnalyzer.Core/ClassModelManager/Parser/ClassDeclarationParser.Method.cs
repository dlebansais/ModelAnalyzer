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
            if (IsMethodDeclarationValid(parsingContext, methodDeclaration, out AccessModifier AccessModifier, out ExpressionType ReturnType))
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
                Local? ResultLocal;
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
                    MethodParsingContext = MethodParsingContext with { HostMethod = TemporaryMethod, IsFieldAllowed = true, IsLocalAllowed = true };

                    StatementList = ParseStatements(MethodParsingContext, methodDeclaration);
                    ResultLocal = FindOrCreateResultLocal(LocalTable, ReturnType);

                    TemporaryMethod = new Method
                    {
                        Name = MethodName,
                        AccessModifier = AccessModifier,
                        ParameterTable = ParameterTable,
                        ReturnType = ReturnType,
                        RequireList = RequireList,
                        LocalTable = LocalTable,
                        ResultLocal = ResultLocal,
                        StatementList = StatementList,
                        EnsureList = new List<Ensure>(),
                    };
                    MethodParsingContext = MethodParsingContext with { HostMethod = TemporaryMethod };

                    EnsureList = ParseEnsures(MethodParsingContext, methodDeclaration);
                }
                else
                {
                    RequireList = new List<Require>();
                    LocalTable = ReadOnlyLocalTable.Empty;
                    ResultLocal = null;
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
                    ResultLocal = ResultLocal,
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

    private bool IsMethodDeclarationValid(ParsingContext parsingContext, MethodDeclarationSyntax methodDeclaration, out AccessModifier accessModifier, out ExpressionType returnType)
    {
        bool IsMethodSupported = true;

        if (methodDeclaration.AttributeLists.Count > 0)
        {
            LogWarning($"Unsupported {methodDeclaration.AttributeLists.Count} method attribute(s).");

            IsMethodSupported = false;
        }

        if (!IsTypeSupported(parsingContext, methodDeclaration.ReturnType, out returnType))
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

        if (methodDeclaration.TypeParameterList is TypeParameterListSyntax TypeParameterList && TypeParameterList.Parameters.Count > 0)
        {
            LogWarning("Unsupported method type parameter(s).");

            IsMethodSupported = false;
        }

        if (methodDeclaration.ConstraintClauses.Count > 0)
        {
            LogWarning("Unsupported method constraint(s).");

            IsMethodSupported = false;
        }

        return IsMethodSupported;
    }

    private void ReportInvalidMethodCalls(ParsingContext parsingContext)
    {
        List<ICallLocation> RemovedCallLocationList = new();
        bool IsErrorReported;

        ParsingContext InvariantParsingContext = parsingContext with { HostMethod = null };
        IsErrorReported = false;
        ReportInvalidMethodCalls(InvariantParsingContext, new List<Method>(), RemovedCallLocationList, ref IsErrorReported);

        foreach (KeyValuePair<MethodName, Method> Entry in parsingContext.MethodTable)
        {
            ParsingContext MethodParsingContext = parsingContext with { HostMethod = Entry.Value };
            IsErrorReported = false;
            ReportInvalidMethodCalls(MethodParsingContext, new List<Method>(), RemovedCallLocationList, ref IsErrorReported);
        }

        foreach (ICallLocation CallLocation in RemovedCallLocationList)
            CallLocation.RemoveCall();
    }

    private void ReportInvalidMethodCalls(ParsingContext parsingContext, List<Method> visitedMethodList, List<ICallLocation> removedCallLocationList, ref bool isErrorReported)
    {
        foreach (MethodCallStatementEntry Entry in parsingContext.MethodCallStatementList)
            if (parsingContext.HostMethod is not null && Entry.HostMethod.Name == parsingContext.HostMethod.Name)
            {
                MethodCallStatement MethodCall = Entry.Statement;

                if (!IsValidMethodCall(parsingContext, visitedMethodList, removedCallLocationList, MethodCall, out Location Location, ref isErrorReported))
                {
                    parsingContext.Unsupported.AddUnsupportedStatement(Location);
                    removedCallLocationList.Add(Entry.CallLocation);
                }
            }

        foreach (FunctionCallStatementEntry Entry in parsingContext.FunctionCallExpressionList)
        {
            bool IsMethodFound = (Entry.HostMethod is Method EntryMethod && parsingContext.HostMethod is Method HostMethod && EntryMethod.Name == HostMethod.Name) ||
                                 (Entry.HostMethod is null && parsingContext.HostMethod is null);

            if (IsMethodFound)
            {
                FunctionCallExpression FunctionCall = Entry.Expression;

                if (!IsValidFunctionCall(parsingContext, visitedMethodList, removedCallLocationList, FunctionCall, out Location Location, ref isErrorReported))
                {
                    if (!isErrorReported)
                    {
                        parsingContext.Unsupported.AddUnsupportedExpression(Location);
                        removedCallLocationList.Add(Entry.CallLocation);
                    }
                }
            }
        }
    }

    private bool IsValidMethodCall(ParsingContext parsingContext, List<Method> visitedMethodList, List<ICallLocation> removedCallLocationList, MethodCallStatement methodCall, out Location location, ref bool isErrorReported)
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

            if (!IsValidMethodCallArgument(parsingContext, Argument, Parameter, ref isErrorReported))
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
        ReportInvalidMethodCalls(CalledMethodParsingContext, NewVisitedMethodList, removedCallLocationList, ref isErrorReported);

        return true;
    }

    private bool IsValidFunctionCall(ParsingContext parsingContext, List<Method> visitedMethodList, List<ICallLocation> removedCallLocationList, FunctionCallExpression functionCall, out Location location, ref bool isErrorReported)
    {
        MethodTable MethodTable = parsingContext.MethodTable;
        Method? HostMethod = parsingContext.HostMethod;

        location = functionCall.NameLocation;

        if (HostMethod is not null && functionCall.FunctionName == HostMethod.Name)
            return false;

        Method? CalledMethod = null;

        foreach (var Entry in MethodTable)
            if (Entry.Key == functionCall.FunctionName)
            {
                CalledMethod = Entry.Value;
                break;
            }

        if (CalledMethod is null)
            return false;

        if (visitedMethodList.Contains(CalledMethod))
            return false;

        int Count = functionCall.ArgumentList.Count;

        if (Count != CalledMethod.ParameterTable.Count)
            return false;

        int Index = 0;
        bool AllArgumentsValid = true;

        foreach (KeyValuePair<ParameterName, Parameter> Entry in CalledMethod.ParameterTable)
        {
            Argument Argument = functionCall.ArgumentList[Index++];
            Parameter Parameter = Entry.Value;

            if (!IsValidMethodCallArgument(parsingContext, Argument, Parameter, ref isErrorReported))
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
        ReportInvalidMethodCalls(CalledMethodParsingContext, NewVisitedMethodList, removedCallLocationList, ref isErrorReported);

        return true;
    }

    private bool IsValidMethodCallArgument(ParsingContext parsingContext, Argument argument, Parameter parameter, ref bool isErrorReported)
    {
        Expression ArgumentExpression = (Expression)argument.Expression;
        ExpressionType ArgumentType = ArgumentExpression.GetExpressionType(parsingContext);

        // If ArgumentType is ExpressionType.Other, this is an unsupported expression.
        if (ArgumentType != ExpressionType.Other)
        {
            ExpressionType ParameterType = parameter.Type;

            if (IsSourceAndDestinationTypeCompatible(ParameterType, ArgumentType))
                return true;
        }
        else
            isErrorReported = true;

        return false;
    }

    private Local? FindOrCreateResultLocal(ReadOnlyLocalTable localTable, ExpressionType returnType)
    {
        Debug.Assert(returnType != ExpressionType.Other);

        Local? ResultLocal;

        if (returnType != ExpressionType.Void)
        {
            LocalName ResultName = new LocalName() { Text = Ensure.ResultKeyword };

            foreach (KeyValuePair<LocalName, Local> Entry in localTable)
                if (Entry.Key == ResultName)
                    return Entry.Value;

            ResultLocal = new Local() { Name = ResultName, Type = returnType, Initializer = null };
        }
        else
            ResultLocal = null;

        return ResultLocal;
    }
}
