﻿namespace ModelAnalyzer;

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
    private ReadOnlyMethodTable ParseMethods(ClassDeclarationSyntax classDeclaration, ReadOnlyFieldTable fieldTable, Unsupported unsupported)
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

        ReportInvalidMethodCalls(MethodTable, unsupported);

        return MethodTable.ToReadOnly();
    }

    private void AddMethod(MethodDeclarationSyntax methodDeclaration, ReadOnlyFieldTable fieldTable, MethodTable methodTable, Unsupported unsupported)
    {
        string Text = methodDeclaration.Identifier.ValueText;
        MethodName MethodName = new() { Text = Text };

        // Ignore duplicate names, the compiler will catch them.
        if (!methodTable.ContainsItem(MethodName))
        {
            if (IsMethodDeclarationValid(methodDeclaration, out AccessModifier AccessModifier, out ExpressionType ReturnType))
            {
                Method TemporaryMethod;

                ReadOnlyParameterTable ParameterTable = ParseParameters(methodDeclaration, fieldTable, unsupported);

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

                List<Require> RequireList = ParseRequires(methodDeclaration, fieldTable, TemporaryMethod, unsupported);
                ReadOnlyLocalTable LocalTable = ParseLocals(methodDeclaration, fieldTable, TemporaryMethod, unsupported);

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

                List<Statement> StatementList = ParseStatements(methodDeclaration, fieldTable, TemporaryMethod, unsupported);

                Local? ResultLocal = null;

                if (ReturnType != ExpressionType.Void)
                {
                    Debug.Assert(ReturnType != ExpressionType.Other);

                    LocalName ResultName = new LocalName() { Text = Ensure.ResultKeyword };

                    foreach (KeyValuePair<LocalName, Local> Entry in LocalTable)
                        if (Entry.Key == ResultName)
                        {
                            ResultLocal = Entry.Value;
                            break;
                        }

                    if (ResultLocal is null)
                        ResultLocal = new Local() { Name = ResultName, Type = ReturnType, Initializer = null };
                }

                List<Ensure> EnsureList = ParseEnsures(methodDeclaration, fieldTable, TemporaryMethod, ResultLocal, unsupported);

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
            else
            {
                if (TryFindTrailingTrivia(methodDeclaration, out SyntaxTriviaList TriviaList))
                    ReportUnsupportedEnsures(unsupported, TriviaList);

                Location Location = methodDeclaration.Identifier.GetLocation();
                unsupported.AddUnsupportedMethod(Location);
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

    private void ReportInvalidMethodCalls(MethodTable methodTable, Unsupported unsupported)
    {
        List<Method> VisitedMethodList = new();

        foreach (KeyValuePair<MethodName, Method> Entry in methodTable)
            ReportInvalidMethodCalls(methodTable, VisitedMethodList, unsupported, Entry.Value);
    }

    private void ReportInvalidMethodCalls(MethodTable methodTable, List<Method> visitedMethodList, Unsupported unsupported, Method method)
    {
        List<Statement> StatementList = method.StatementList;
        int i = 0;

        while (i < StatementList.Count)
        {
            if (StatementList[i] is MethodCallStatement MethodCall)
            {
                if (!IsValidMethodCall(methodTable, visitedMethodList, unsupported, method, MethodCall, out Location Location))
                {
                    unsupported.AddUnsupportedStatement(Location);
                    StatementList.RemoveAt(i);
                }
                else
                    i++;
            }
            else
                i++;
        }
    }

    private bool IsValidMethodCall(MethodTable methodTable, List<Method> visitedMethodList, Unsupported unsupported, Method methodHost, MethodCallStatement methodCall, out Location location)
    {
        location = methodCall.NameLocation;

        if (methodCall.MethodName == methodHost.Name)
            return false;

        Method? CalledMethod = null;

        foreach (var Entry in methodTable)
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

            if (!IsValidMethodCallArgument(methodHost, Argument, Parameter))
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

        ReportInvalidMethodCalls(methodTable, NewVisitedMethodList, unsupported, CalledMethod);

        return true;
    }

    private bool IsValidMethodCallArgument(Method hostMethod, Argument argument, Parameter parameter)
    {
        Expression ArgumentExpression = (Expression)argument.Expression;
        ExpressionType ArgumentType = ArgumentExpression.GetExpressionType(FieldTable, hostMethod, resultLocal: null);
        ExpressionType ParameterType = parameter.Type;

        if (!IsSourceAndDestinationTypeCompatible(ParameterType, ArgumentType))
            return false;

        return true;
    }
}
