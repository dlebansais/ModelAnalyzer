﻿namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private MethodTable ParseMethods(ClassDeclarationSyntax classDeclaration, FieldTable fieldTable, Unsupported unsupported)
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

        MethodTable.Seal();

        return MethodTable;
    }

    private void AddMethod(MethodDeclarationSyntax methodDeclaration, FieldTable fieldTable, MethodTable methodTable, Unsupported unsupported)
    {
        string Name = methodDeclaration.Identifier.ValueText;
        MethodName MethodName = new(Name);

        // Ignore duplicate names, the compiler will catch them.
        if (!methodTable.ContainsItem(MethodName))
        {
            IMethod NewMethod;

            if (IsMethodDeclarationValid(methodDeclaration, out bool HasReturnValue))
            {
                bool IsSupported = true;
                ParameterTable ParameterTable = ParseParameters(methodDeclaration, fieldTable, unsupported);
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
                if (TryFindTrailingTrivia(methodDeclaration, out SyntaxTriviaList TriviaList))
                    ReportUnsupportedEnsures(unsupported, TriviaList);

                Location Location = methodDeclaration.Identifier.GetLocation();
                unsupported.AddUnsupportedMethod(Location, out IUnsupportedMethod UnsupportedMethod);

                NewMethod = UnsupportedMethod;
            }

            methodTable.AddItem(MethodName, NewMethod);
        }
    }

    private bool IsMethodDeclarationValid(MethodDeclarationSyntax methodDeclaration, out bool hasReturnValue)
    {
        bool IsMethodSupported = true;

        if (methodDeclaration.AttributeLists.Count > 0)
        {
            LogWarning($"Unsupported {methodDeclaration.AttributeLists.Count} method attribute(s).");

            IsMethodSupported = false;
        }

        if (!IsTypeSupported(methodDeclaration.ReturnType, out bool IsVoidReturn))
        {
            LogWarning($"Unsupported method return type.");

            hasReturnValue = false;
            IsMethodSupported = false;
        }
        else
            hasReturnValue = !IsVoidReturn;

        foreach (SyntaxToken Modifier in methodDeclaration.Modifiers)
            if (!Modifier.IsKind(SyntaxKind.PrivateKeyword) && !Modifier.IsKind(SyntaxKind.PublicKeyword) && !Modifier.IsKind(SyntaxKind.InternalKeyword))
            {
                LogWarning($"Unsupported '{Modifier.ValueText}' method modifier.");

                IsMethodSupported = false;
            }

        return IsMethodSupported;
    }

    private ParameterTable ParseParameters(MethodDeclarationSyntax methodDeclaration, FieldTable fieldTable, Unsupported unsupported)
    {
        ParameterTable ParameterTable = new();

        foreach (ParameterSyntax Parameter in methodDeclaration.ParameterList.Parameters)
        {
            ParameterName ParameterName = new(Parameter.Identifier.ValueText);

            // Ignore duplicate names, the compiler will catch them.
            if (!ParameterTable.ContainsItem(ParameterName))
            {
                IParameter NewParameter;

                if (IsParameterSupported(Parameter, fieldTable))
                {
                    NewParameter = new Parameter() { ParameterName = ParameterName };
                }
                else
                {
                    Location Location = Parameter.GetLocation();
                    unsupported.AddUnsupportedParameter(Location, out IUnsupportedParameter UnsupportedParameter);

                    NewParameter = UnsupportedParameter;
                }

                ParameterTable.AddItem(ParameterName, NewParameter);
            }
        }

        ParameterTable.Seal();

        return ParameterTable;
    }

    private bool IsParameterSupported(ParameterSyntax parameter, FieldTable fieldTable)
    {
        bool IsParameterSupported = true;

        if (parameter.AttributeLists.Count > 0)
        {
            LogWarning($"Unsupported {parameter.AttributeLists.Count} parameter attribute(s).");

            IsParameterSupported = false;
        }

        if (parameter.Modifiers.Count > 0)
        {
            LogWarning($"Unsupported {parameter.Modifiers.Count} parameter modifier(s).");

            IsParameterSupported = false;
        }

        if (!IsTypeSupported(parameter.Type, out _))
        {
            LogWarning($"Unsupported parameter type.");

            IsParameterSupported = false;
        }

        string ParameterName = parameter.Identifier.ValueText;

        if (TryFindFieldByName(fieldTable, ParameterName, out _))
        {
            LogWarning($"Parameter '{ParameterName}' is already the name of a field.");

            IsParameterSupported = false;
        }

        return IsParameterSupported;
    }

    private bool TryFindParameterByName(ParameterTable parameterTable, string parameterName, out IParameter parameter)
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
}
